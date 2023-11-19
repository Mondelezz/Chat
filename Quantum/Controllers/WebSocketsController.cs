using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quantum.Interfaces.UserInterface;
using Quantum.Interfaces.WebSocketInterface;
using Quantum.Services;
using System.Net.WebSockets;
using System.Text;

namespace Quantum.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebSocketsController : ControllerBase
    {
        private readonly IWebSocket _webSocket;
        private readonly IWebSocketToClient _webSocketToClient;
        private readonly ILogger<WebSocketsController> _logger;
        private readonly JwtTokenProcess _jwtTokenProcess;
        private readonly ICheckingDataChange _checkingDataChange;
        public WebSocketsController(ILogger<WebSocketsController> logger, IWebSocket webSocket, JwtTokenProcess jwtTokenProcess, IWebSocketToClient webSocketToClient, ICheckingDataChange checkingDataChange)
        {
            _logger = logger;
            _webSocket = webSocket;
            _jwtTokenProcess = jwtTokenProcess;
            _webSocketToClient = webSocketToClient;
            _checkingDataChange = checkingDataChange;
        }

        // Получение токена из заголовка авторизации
        private string ExtractAuthTokenFromHeaders()
        {
            try
            {
                string token = HttpContext.Request.Headers["Authorization"]!;
                return token;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"Заголовок отсутствует{ ex.Message}");
                throw;
            }

        }
        // Получение номера отправителя
        private string GetSenderPhoneNumber(string token)
        {
            try
            {
                string senderPhoneNumber = _jwtTokenProcess.GetPhoneNumberFromJwtToken(token);
                _logger.Log(LogLevel.Information, $"Номер телефона отправителя: {senderPhoneNumber} \n");

                return senderPhoneNumber;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"Номер телефона отправителя отсутствует. {ex.Message} \n");
                throw;
            }                     
        }
        // Получение номера получателя
        private string GetReceivePhoneNumber()
        {
            try
            {
                string receiverPhoneNumber = HttpContext.Request.Query["receiverPhoneNumber"]!;
                _logger.Log(LogLevel.Information, $"Номер телефона получателя: {receiverPhoneNumber}  \n");

                return receiverPhoneNumber;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"Номер телефона получателя отсутствует. {ex.Message}  \n");
                throw;
            }
        }
        /// <summary>
        /// Управление вебсокет соединениями
        /// </summary>
        /// <returns></returns>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("send")]        
        public async Task<IActionResult> HandleNewWebSocketConnection()
        {
            try
            {
                // Токен из заголовка запроса
                string token = ExtractAuthTokenFromHeaders();
                // Веб-сокет соединение
                WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                // Отправитель
                string senderPhoneNumber = GetSenderPhoneNumber(token);

                Dictionary<string, List<WebSocket>> phoneToWebSockets = _webSocketToClient.AddWebSocketToClient(webSocket, senderPhoneNumber);

                await ProcessWebSocketMessages(webSocket, senderPhoneNumber, token, phoneToWebSockets);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка подключения к веб-сокет соединению: {ex.Message}\n");
                throw;
            }
        }

        /// <summary>
        ///  Мониторинг состояния соединения и обеспечивает чтение данных, переданных по веб-сокет соединению
        /// </summary>
        /// <param name="webSocket"></param>
        /// Веб-сокет соединение
        /// <param name="senderPhoneNumber"></param>
        /// Номер отправителя
        /// <param name="token"></param>
        /// jwt токен
        /// <param name="phoneToWebSockets"></param>
        /// Словарь номеров телефонов и их сокет соединений
        /// <returns></returns>
        private async Task ProcessWebSocketMessages(WebSocket webSocket, string senderPhoneNumber, string token, Dictionary<string, List<WebSocket>> phoneToWebSockets)
        {
            // Получатель
            string receiverPhoneNumber = GetReceivePhoneNumber();

            while (webSocket.State == WebSocketState.Open)
            {
                ArraySegment<byte> buffers = new ArraySegment<byte>(new byte[4096]);
                WebSocketReceiveResult receiveResult = await webSocket.ReceiveAsync(buffers, CancellationToken.None);
                if (receiveResult.MessageType == WebSocketMessageType.Text && receiveResult.EndOfMessage)
                {
                    await ProcessTextMessage(webSocket, senderPhoneNumber, receiverPhoneNumber, buffers, receiveResult, token, phoneToWebSockets);
                }
            }
            if (webSocket.State == WebSocketState.CloseSent)
            {
                _logger.Log(LogLevel.Information, $"Соединение закрыто успешно.\n");
            }
            
        }
        /// <summary>
        /// Процесс отправки сообщения пользователю, а также обеспечение закрытие соединения при изменении данных пользователя и отмену отправки сообщения
        /// </summary>
        /// <param name="webSocket"></param>
        /// Веб-сокет соединение
        /// <param name="senderPhoneNumber"></param>
        /// Номер отправителя 
        /// <param name="receiverPhoneNumber"></param>
        /// Номер получателя
        /// <param name="buffers"></param>
        /// Сегмент данных
        /// <param name="receiveResult"></param>
        /// Объект содержит информацию о сообщении: Count : длину, MessageType: тип, EndOfMessage: является ли сообщение завершенным
        /// <param name="token"></param>
        /// jwt токен
        /// <param name="phoneToWebSockets"></param>
        /// Словарь номеров телефонов и их сокет соединений
        /// <returns></returns>
        private async Task ProcessTextMessage(WebSocket webSocket, string senderPhoneNumber, string receiverPhoneNumber, ArraySegment<byte> buffers, WebSocketReceiveResult receiveResult, string token, Dictionary<string, List<WebSocket>> phoneToWebSockets)
        {
            bool result = await _checkingDataChange.CheckingDataChangeAsync(token, senderPhoneNumber);
            if (result)
            {
                await _webSocketToClient.CloseWebSocketConnection(webSocket, senderPhoneNumber);
            }
            else
            {
                byte[] receivedBuffers = buffers.Skip(count: buffers.Offset)
                                                .Take(count: receiveResult.Count)
                                                .ToArray();

                await _webSocket.SendMessageToUser(senderPhoneNumber, receiverPhoneNumber, receivedBuffers, phoneToWebSockets);
                _logger.Log(LogLevel.Information, $"Сообщение отправлено успешно.\n\t{senderPhoneNumber}: {Encoding.UTF8.GetString(receivedBuffers)}\n");
            }
        }

    }
}
