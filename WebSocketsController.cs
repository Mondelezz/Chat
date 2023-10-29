using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
using System.Text;

namespace Quantum.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebSocketsController : ControllerBase
    {
        private readonly ILogger<WebSocketsController> _logger;
        public WebSocketsController(ILogger<WebSocketsController> logger)
        {
            _logger = logger;
        }

        [HttpGet("ws")]
        public async Task GetWebSocket()
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
                _logger.Log(LogLevel.Information, "WebSockets connection established");
                await Echo(webSocket);
            }
            else
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }

        /// <summary>
        /// Метод Эхо.
        /// </summary>
        /// <param name="httpContext"> 
        /// Принятие сведений о запросе
        /// </param>
        /// <param name="webSocket">
        /// Объект содержит websocket - соединение.
        /// </param>
        private async Task Echo(WebSocket webSocket)
        {
            // Массив, содержащий данные для отправки через веб-сокет.
            byte[] buffer = new byte[4096];

            /// <summary>
            /// result содержит информацию чтения данных из веб-сокета.
            /// </summary>
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            _logger.Log(LogLevel.Information, "Сообщение, полученное от клиента");

            while (!result.CloseStatus.HasValue)
            {
                byte[] serverMsg = Encoding.UTF8.GetBytes($"Server: Привет. Ты сказал {Encoding.UTF8.GetString(buffer)}");

                /// <summary>
                /// Отправка данных через веб-сокет.
                /// ArraySegment обеспечивает частичную отправку данных, указывая диапазон offset(смещение) и count(количество элементов).
                /// </summary>
                await webSocket.SendAsync(new ArraySegment<byte>(serverMsg, offset: 0, count: serverMsg.Length), result.MessageType, result.EndOfMessage, CancellationToken.None);
                _logger.Log(LogLevel.Information, "Сообщение, отправленное клиенту");

                buffer = new byte[4096];

                /// <summary>
                /// Чтение данных из веб-сокет соединения.
                /// </summary>
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                _logger.Log(LogLevel.Information, "Сообщение полученное от клиента");
            }
            /// <summary>
            /// Закрытие веб-сокет соединения.
            /// </summary>
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }
    }
}
