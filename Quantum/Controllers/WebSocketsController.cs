using Microsoft.AspNetCore.Mvc;
using Quantum.Interfaces;
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
        public WebSocketsController(ILogger<WebSocketsController> logger, IWebSocket webSocket)
        {
            _logger = logger;
            _webSocket = webSocket;
        }

        [HttpGet("message/get")]
        public async Task GetWebSocketAsync()
        {
            /// <summary>
            ///  Если HttpContext запрос это WebSocket запрос.
            /// </summary>
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                /// <summary>
                /// Ожидание установления WebSocket - соединения.
                /// </summary>
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                _logger.Log(LogLevel.Information, "Установлено соединение через WebSocket");
                await _webSocket.HandleWebSocketRequestAsync(webSocket);
            }
            else
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }               
        
    }
}
