using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quantum.Services;
using Quantum.UserP.UserInterface;
using System.Buffers;
using System.Net.WebSockets;
using System.Text;
using Quantum.Interfaces.WebSocketInterface;
using Quantum.Services.WebSocketServices;

namespace Quantum.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebSocketsController : ControllerBase
    {
        private readonly IWebSocket _webSocket;
        private readonly IWebSocketToClient _webSocketToClient;
        private readonly ILogger<WebSocketsController> _logger;
        private readonly ILogger<WebSocketReceiveResultProcessor> _logger2;
        private readonly JwtTokenProcess _jwtTokenProcess;
        private readonly ICheckingDataChange _checkingDataChange;
        public WebSocketsController(
            ILogger<WebSocketsController> logger,
            ILogger<WebSocketReceiveResultProcessor> logger2,
            IWebSocket webSocket,
            JwtTokenProcess jwtTokenProcess,
            IWebSocketToClient webSocketToClient,
            ICheckingDataChange checkingDataChange)
        {
            _logger = logger;
            _logger2= logger2;
            _webSocket = webSocket;
            _jwtTokenProcess = jwtTokenProcess;
            _webSocketToClient = webSocketToClient;
            _checkingDataChange = checkingDataChange;
        }

        /// <summary>
        /// Управление вебсокет соединениями
        /// </summary>
        /// <returns></returns>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("send")]
        public async Task HandleNewWebSocketConnection()
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

            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка подключения к веб-сокет соединению: {ex.Message}\n");
                throw new Exception(ex.ToString());
            }
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
        private async Task ProcessWebSocketMessages(
            WebSocket webSocket,           
            string senderPhoneNumber,
            string token,
            Dictionary<string, List<WebSocket>> phoneToWebSockets)
        {
            // Получатель
            string receiverPhoneNumber = GetReceivePhoneNumber();
            while (webSocket.State == WebSocketState.Open)
            {
                //Channel<byte[]> channel = Channel.CreateBounded<byte[]>(new BoundedChannelOptions(100)
                //{
                //    FullMode = BoundedChannelFullMode.DropOldest
                //});
                byte[] memory = new byte[4096];
                ArraySegment<byte> buffer = new ArraySegment<byte>(memory);
                _logger.Log(LogLevel.Information, $"buffers: {buffer}");
                try
                {
                    WebSocketReceiveResult receiveResult = await webSocket.ReceiveAsync(buffer, CancellationToken.None);
                    if (receiveResult.Count == 0)
                    {
                        _logger.Log(LogLevel.Warning, "\nreceiveResult ПУСТ\n");
                        await _webSocketToClient.CloseWebSocketConnectionAsync(webSocket, senderPhoneNumber);
                    }
                    _logger.Log(LogLevel.Information, $"receiveResult: {receiveResult.Count}");

                    WebSocketReceiveResultProcessor resultProcessor = new WebSocketReceiveResultProcessor(_logger2);
                    bool isEndOfMessage = resultProcessor.Receive(receiveResult, buffer, out var frame);
                    if (isEndOfMessage)
                    {
                        if (frame.IsEmpty == true)
                        {
                            break;
                        }
                        else
                        {
                            _logger.Log(LogLevel.Information, $"{frame}");
                            await ProcessTextMessage(webSocket, frame, senderPhoneNumber, receiverPhoneNumber, token, phoneToWebSockets);
                        }
                    }
                    else
                    {
                        await ProcessSingleTextMessage(webSocket, senderPhoneNumber, buffer, receiveResult, receiverPhoneNumber, token, phoneToWebSockets);
                    }
                }
                catch (WebSocketException ex)
                {
                    _logger.LogError(ex, ex.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, ex.Message);
                    return;
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(buffer.ToArray()!);

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
        private async Task ProcessTextMessage(WebSocket webSocket, ReadOnlySequence<byte> frame, string senderPhoneNumber, string receiverPhoneNumber,  string token, Dictionary<string, List<WebSocket>> phoneToWebSockets)
        {
            foreach (var segment in frame)
            {
                Console.WriteLine("\n\n------------------------------");
                Console.WriteLine(segment);
                Console.WriteLine("------------------------------\n\n");
                bool result = await _checkingDataChange.CheckingDataChangeAsync(token, senderPhoneNumber);
                if (result)
                {
                    await _webSocketToClient.CloseWebSocketConnectionAsync(webSocket, senderPhoneNumber);
                }
                else
                {

                    byte[] receivedBuffers = segment.ToArray();

                    bool resultSendMessage = await _webSocket.SendMessageToUserAsync(senderPhoneNumber, receiverPhoneNumber, receivedBuffers, phoneToWebSockets);
                    if (resultSendMessage)
                    {
                        _logger.Log(LogLevel.Information, $"Сообщение отправлено успешно.\n\t{senderPhoneNumber}: {Encoding.UTF8.GetString(receivedBuffers)}\n");
                    }
                    else
                    {
                        _logger.Log(LogLevel.Warning, $"Получателя нет, отправить некому.");

                        await _webSocketToClient.CloseWebSocketConnectionAsync(webSocket, senderPhoneNumber);
                    }
                }
            }
            
        }
        private async Task ProcessSingleTextMessage(WebSocket webSocket, string senderPhoneNumber, ArraySegment<byte> buffers, WebSocketReceiveResult receiveResult, string receiverPhoneNumber, string token, Dictionary<string, List<WebSocket>> phoneToWebSockets)
        {
            bool result = await _checkingDataChange.CheckingDataChangeAsync(token, senderPhoneNumber);
            if (result)
            {
                await _webSocketToClient.CloseWebSocketConnectionAsync(webSocket, senderPhoneNumber);
            }
            else
            {

                byte[] receivedBuffers = buffers.Skip(count: buffers.Offset)
                                               .Take(count: receiveResult.Count)
                                               .ToArray();

                bool resultSendMessage = await _webSocket.SendMessageToUserAsync(senderPhoneNumber, receiverPhoneNumber, receivedBuffers, phoneToWebSockets);
                if (resultSendMessage)
                {
                    _logger.Log(LogLevel.Information, $"Сообщение отправлено успешно.\n\t{senderPhoneNumber}: {Encoding.UTF8.GetString(receivedBuffers)}\n");
                }
                else
                {
                    _logger.Log(LogLevel.Warning, $"Получателя нет, отправить некому.");

                    await _webSocketToClient.CloseWebSocketConnectionAsync(webSocket, senderPhoneNumber);
                }
            }

        }
    }
}
