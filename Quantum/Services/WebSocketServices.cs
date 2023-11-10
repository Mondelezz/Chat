using Microsoft.AspNetCore.Authorization;
using Quantum.Interfaces;
using System.Net.WebSockets;
using System.Text;

namespace Quantum.Services
{
    public class WebSocketServices : IWebSocket
    {
        private readonly ILogger<WebSocketServices> _logger;
        private readonly IJwtTokenProcess _jwtTokenProcess;
        private static readonly ReaderWriterLockSlim Locker = new ReaderWriterLockSlim();
        private readonly Dictionary<string, List<WebSocket>> PhoneToWebSockets = new Dictionary<string, List<WebSocket>>();

        public WebSocketServices(ILogger<WebSocketServices> logger, IJwtTokenProcess jwtTokenProcess)
        {
            _logger = logger;
            _jwtTokenProcess = jwtTokenProcess;
        }

        /// <summary>
        /// Метод обеспечивает связывание WebSocket-соединений с конкретными
        /// пользователями по их номерам телефонов. Если это первое соединение пользователя, 
        /// создается новый список для хранения всех его соединений. Последующие соединения 
        /// добавляются в список пользователя в соответствии с его номером телефона
        /// </summary>
        /// <param name="webSocket"></param>
        public void AddWebSocketToClient(WebSocket webSocket)
        {
            Locker.EnterWriteLock();
            try
            {
                string phoneNumber = _jwtTokenProcess.GetPhoneNumberFromJwtToken();
                if (!PhoneToWebSockets.ContainsKey(phoneNumber))
                {
                    // Если первое соединение - создаем список для хранения веб-сокет соединений.
                    PhoneToWebSockets[phoneNumber] = new List<WebSocket>();
                }
                // Добавляем новое соединение WebSocket к списку соединений пользователя
                PhoneToWebSockets[phoneNumber].Add(webSocket);
                // Добавляем сокет клиента в список клиентов
            }
            finally
            {
                // Гарантируется, что вызываемый объект выходит из режима записи
                Locker.ExitWriteLock();
            }
        }
        
        public async Task SendMessageToUser(string senderPhoneNumber, string receiverPhoneNumber, byte[] messageBytes)
        {
            try
            {
                if (PhoneToWebSockets.ContainsKey(receiverPhoneNumber))
                {
                    List<WebSocket> userWebSockets = PhoneToWebSockets[receiverPhoneNumber];

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
