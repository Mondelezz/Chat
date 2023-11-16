using Quantum.Interfaces.WebSocketInterface;
using System.Net.WebSockets;
using System.Text;

namespace Quantum.Services.WebSocketServices
{
    public class WebSocketServices : IWebSocket
    {
        private readonly ILogger<WebSocketServices> _logger;
        
        public WebSocketServices(ILogger<WebSocketServices> logger)
        {
            _logger = logger;
        }       

        public async Task SendMessageToUser(string senderPhoneNumber, string receiverPhoneNumber, byte[] messageBytes, Dictionary<string, List<WebSocket>> phoneToWebSockets)
        {
            
            try
            {
                if (phoneToWebSockets.ContainsKey(receiverPhoneNumber))
                {
                    List<WebSocket> userWebSockets = phoneToWebSockets[receiverPhoneNumber];

                    foreach (var userWebSocket in userWebSockets)
                    {
                        if (userWebSocket.State == WebSocketState.Open)
                        {
                            await userWebSocket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, CancellationToken.None);
                            
                        }
                        else
                        {
                            _logger.Log(LogLevel.Warning, "Соединение веб-сокет отсутствует");
                        }
                    }
                }
                else
                {
                    _logger.Log(LogLevel.Warning, "Пользователь не в сети");
                }
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"Ошибка отправки сообщения от пользователя по номеру телефона {senderPhoneNumber} пользователю по номеру телефона {receiverPhoneNumber}: {ex.Message}");
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
        public async Task EchoAsync(WebSocket webSocket)
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

        /// <summary>
        /// Управленние веб-сокет соединениями
        /// </summary>
        //public async Task HandleWebSocketRequestAsync(WebSocket webSocket)
        //{
        //    AddWebSocketToClient(webSocket);           
        //    try
        //    {
        //        while (webSocket.State == WebSocketState.Open)
        //        {
        //            ArraySegment<byte> buffers = new ArraySegment<byte>(new byte[4096]);
        //            /// <summary>
        //            /// result содержит информацию чтения данных из веб-сокета.
        //            /// </summary>
        //            WebSocketReceiveResult result = await webSocket.ReceiveAsync(buffers, CancellationToken.None);
        //            if (result.MessageType == WebSocketMessageType.Text && result.EndOfMessage)
        //            {
        //                byte[] receivedBytes = buffers.Skip(buffers.Offset).Take(result.Count).ToArray();

        //                await BroadcastMessageAsync(receivedBytes);
        //            }                  
        //        }
        //    }           
        //    catch (Exception ex)
        //    {
        //        _logger.Log(LogLevel.Error, $"Исключение: {ex.Message}");
        //        RemoveWebSocketFromClients(webSocket);
        //    }
        //} 

        //private async Task BroadcastMessageAsync(byte[] message)
        //{
        //    Locker.EnterWriteLock();
        //    try
        //    {
        //        // Отправить сообщение всем подключенным клиентам
        //        foreach (var client in Clients)
        //        {
        //            if (client.State == WebSocketState.Open)
        //            {
        //                ArraySegment<byte> buffer = new ArraySegment<byte>(message);
        //                await client.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
        //                _logger.Log(LogLevel.Information, "Сообщение было отправлено клиенту");
        //            }
        //        }
        //    }
        //    finally
        //    {
        //        Locker.ExitWriteLock();
        //    }
        //}
    }
}
