using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.IIS.Core;
using Microsoft.IdentityModel.Tokens;
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

        public WebSocketsController(ILogger<WebSocketsController> logger, IWebSocket webSocket, JwtTokenProcess jwtTokenProcess, IWebSocketToClient webSocketToClient)
        {
            _logger = logger;
            _webSocket = webSocket;
            _jwtTokenProcess = jwtTokenProcess;
            _webSocketToClient = webSocketToClient;
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
                _logger.Log(LogLevel.Information, $"Номер телефона отправителя: {senderPhoneNumber}");
                return senderPhoneNumber;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"Номер телефона отправителя отсутствует. {ex.Message}");
                throw;
            }                     
        }
        // Получение номера получателя
        private string GetReceivePhoneNumber()
        {
            try
            {
                string receiverPhoneNumber = HttpContext.Request.Query["receiverPhoneNumber"]!;
                _logger.Log(LogLevel.Information, $"Номер телефона получателя: {receiverPhoneNumber}");
                return receiverPhoneNumber;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"Номер телефона получателя отсутствует. {ex.Message}");
                throw;
            }
        }
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("send")]
        public async Task<ActionResult> HandleNewWebSocketConnection()
        {
            try
            {
                // Токен из заголовка запроса
                string token = ExtractAuthTokenFromHeaders();

                WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

                // Отправитель
                string senderPhoneNumber = GetSenderPhoneNumber(token);

                Dictionary<string, List<WebSocket>> phoneToWebSockets = _webSocketToClient.AddWebSocketToClient(webSocket, senderPhoneNumber);

                // Получатель
                string receiverPhoneNumber = GetReceivePhoneNumber();

                // Осуществляю отправку сообщения
                try
                {
                    while (webSocket.State == WebSocketState.Open)
                    {
                        ArraySegment<byte> buffers = new ArraySegment<byte>(new byte[4096]);
                        WebSocketReceiveResult result = await webSocket.ReceiveAsync(buffers, CancellationToken.None);
                        if (result.MessageType == WebSocketMessageType.Text && result.EndOfMessage)
                        {
                            byte[] receivedBuffers = buffers.Skip(buffers.Offset).Take(result.Count).ToArray();
                            await _webSocket.SendMessageToUser(senderPhoneNumber, receiverPhoneNumber, receivedBuffers, phoneToWebSockets);
                            _logger.Log(LogLevel.Information, $"Сообщение отправлено успешно.{Encoding.UTF8.GetString(receivedBuffers)}");
                        }                      
                    }
                }
                catch (Exception)
                {
                    throw;
                }
                await webSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, webSocket.CloseStatusDescription, CancellationToken.None);
                return Ok("Сообщение отправлено успешно.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Сообщение об ошибке отправки: {ex.Message}");
                return StatusCode(500, "Ошибка");
            }
        }
        
    }
}
