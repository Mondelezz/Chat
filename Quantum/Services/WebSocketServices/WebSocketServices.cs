using Microsoft.EntityFrameworkCore;
using Quantum.Data;
using Quantum.Interfaces.WebSocketInterface;
using Quantum.Models;
using Quantum.Models.DTO;
using System.Buffers;
using System.Net.WebSockets;
using System.Text;

namespace Quantum.Services.WebSocketServices
{
    public class WebSocketServices : IWebSocket
    {
        private readonly ILogger<WebSocketServices> _logger;
        private readonly DataContext _dataContext;
        public WebSocketServices(ILogger<WebSocketServices> logger, DataContext dataContext)
        {
            _logger = logger;
            _dataContext = dataContext;
        }       
        public async Task<bool> SendMessageToUserAsync(string senderPhoneNumber, string receiverPhoneNumber, byte[] messageBytes, Dictionary<string, List<WebSocket>> phoneToWebSockets)
        {
            List<User> users = _dataContext.Users
                .AsNoTracking()
                .Where(pN => pN.PhoneNumber == senderPhoneNumber || pN.PhoneNumber == receiverPhoneNumber)
                .ToList();

            User? userSender = users.FirstOrDefault(pN => pN.PhoneNumber == senderPhoneNumber);
            User? userReceiver = users.FirstOrDefault(pN => pN.PhoneNumber == receiverPhoneNumber);

            if (userSender == null || userReceiver == null)
            {
                throw new Exception("Пользователь не найден.");
            }
            try
            {
                if (phoneToWebSockets.ContainsKey(receiverPhoneNumber))
                {
                    List<WebSocket> userReceiverWebSockets = phoneToWebSockets[receiverPhoneNumber];
                    foreach (var userReceiverWebSocket in userReceiverWebSockets)
                    {
                        if (userReceiverWebSocket.State == WebSocketState.Open)
                        {
                            await userReceiverWebSocket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, CancellationToken.None);
                            await SaveMessageInDataBaseAsync(userSender, userReceiver, messageBytes);
                        }                                                            
                    }                  
                    return true;
                }
                else
                {
                    _logger.Log(LogLevel.Warning, $"Не существует пользователя с телефоном: {receiverPhoneNumber}\n");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"Ошибка отправки сообщения от пользователя по номеру телефона {senderPhoneNumber} пользователю по номеру телефона {receiverPhoneNumber}: {ex.Message}\n");
                return false;
            }

        }       

        public async Task SaveMessageInDataBaseAsync(User userSender, User userReceiver, byte[] messageBytes)
        {

            TextMessage messages = new TextMessage();
            messages.SenderPhoneNumber = userSender.PhoneNumber;           
            messages.SenderUserId = userSender.UserId;
            
            messages.Message = Encoding.UTF8.GetString(messageBytes);
            messages.SentTime = DateTime.UtcNow;

            messages.ReceiverPhoneNumber = userReceiver.PhoneNumber;
            messages.ReceiverUserId = userReceiver.UserId;

            _dataContext.Messages.Add(messages);
            await _dataContext.SaveChangesAsync();

            _logger.Log(LogLevel.Information, "Данные сохранены успешно.");
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
