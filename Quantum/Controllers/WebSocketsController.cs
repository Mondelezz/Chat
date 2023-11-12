using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Quantum.Interfaces.WebSocketInterface;
using Quantum.Models.DTO;
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

       
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("send")]
        public async Task<ActionResult> HandleNewWebSocketConnection()
        {
            try
            {

                // Токен из заголовка запроса
                var token = HttpContext.Request.Headers["Authorization"];
                if (token.IsNullOrEmpty())
                {
                    _logger.Log(LogLevel.Warning, "Заголовок отсутствует");
                    return BadRequest("HttpContext заголовка не был получен.");
                }

                WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

                // Отправитель
                string senderPhoneNumber = _jwtTokenProcess.GetPhoneNumberFromJwtToken(token);
                if (senderPhoneNumber.IsNullOrEmpty())
                {
                    _logger.Log(LogLevel.Warning, "Номер телефона отправителя отсутствует.");
                    return BadRequest("Номер телефона отправителя не был получен.");
                }
                _logger.Log(LogLevel.Information, $"Номер телефона отправителя: {senderPhoneNumber}");

                Dictionary<string, List<WebSocket>> phoneToWebSockets = _webSocketToClient.AddWebSocketToClient(webSocket, senderPhoneNumber);

                // Получатель
                string receiverPhoneNumber = HttpContext.Request.Query["receiverPhoneNumber"];
                if (receiverPhoneNumber.IsNullOrEmpty())
                {
                    _logger.Log(LogLevel.Warning, "Номер телефона получателя отсутствует.");
                    return BadRequest("Номер телефона получателя не был получен.");
                }
                _logger.Log(LogLevel.Information, $"Номер телефона получателя: {receiverPhoneNumber}");

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
