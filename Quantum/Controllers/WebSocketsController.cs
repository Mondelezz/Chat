using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Quantum.Interfaces;
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
        private readonly ILogger<WebSocketsController> _logger;
        private readonly JwtTokenProcess _jwtTokenProcess;
        public WebSocketsController(ILogger<WebSocketsController> logger, IWebSocket webSocket, JwtTokenProcess jwtTokenProcess)
        {
            _logger = logger;
            _webSocket = webSocket;
            _jwtTokenProcess = jwtTokenProcess;
        }

       
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("send")]
        public async Task<ActionResult> HandleNewWebSocketConnection()
        {
            try
            {
                var token = HttpContext.Request.Headers["Authorization"];
                if (token.IsNullOrEmpty())
                {
                    _logger.Log(LogLevel.Warning, "Заголовок отсутствует");
                    return BadRequest("HttpContext заголовка не был получен.");
                }

                WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                string senderPhoneNumber = _jwtTokenProcess.GetPhoneNumberFromJwtToken(token);
                if (senderPhoneNumber.IsNullOrEmpty())
                {
                    _logger.Log(LogLevel.Warning, "Номер телефона отсутствует.");
                    return BadRequest("Номер телефона не был получен.");
                }
                _logger.Log(LogLevel.Warning, $"Номер телефона отправителя: {senderPhoneNumber}");


                _webSocket.AddWebSocketToClient(webSocket, senderPhoneNumber);
                string receiverPhoneNumber = HttpContext.Request.Query["receiverPhoneNumber"];
                if (receiverPhoneNumber.IsNullOrEmpty())
                {
                    _logger.Log(LogLevel.Warning, "Номер телефона получателя отсутствует.");
                    return BadRequest("Номер телефона получателя не был получен.");
                }
                _logger.Log(LogLevel.Warning, $"Номер телефона получателя: {receiverPhoneNumber}");
                try
                {
                    while (webSocket.State == WebSocketState.Open)
                    {
                        ArraySegment<byte> buffers = new ArraySegment<byte>(new byte[4096]);
                        WebSocketReceiveResult result = await webSocket.ReceiveAsync(buffers, CancellationToken.None);
                        if (result.MessageType == WebSocketMessageType.Text && result.EndOfMessage)
                        {
                            byte[] receivedBuffers = buffers.Skip(buffers.Offset).Take(result.Count).ToArray();
                            await _webSocket.SendMessageToUser(senderPhoneNumber, receiverPhoneNumber, receivedBuffers);
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



        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        //[HttpGet("message/get")]
        //public async Task GetWebSocketAsync()
        //{
        //    /// <summary>
        //    ///  Если HttpContext запрос это WebSocket запрос.
        //    /// </summary>
        //    if (HttpContext.WebSockets.IsWebSocketRequest)
        //    {
        //        /// <summary>
        //        /// Ожидание установления WebSocket - соединения.
        //        /// </summary>
        //        using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
        //        _logger.Log(LogLevel.Information, "Установлено соединение через WebSocket");
        //        await _webSocket.HandleWebSocketRequestAsync(webSocket);
        //    }
        //    else
        //    {
        //        HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        //    }
        //}
        //
    }
}
