using Quantum.Interfaces.WebSocketInterface;
using System.Net.WebSockets;

namespace Quantum.Services.WebSocketServices
{
    public class PhoneToSocketConnectionService : IWebSocketToClient
    {
        private readonly ILogger<PhoneToSocketConnectionService> _logger;
        private static readonly ReaderWriterLockSlim Locker = new ReaderWriterLockSlim();
        public readonly Dictionary<string, List<WebSocket>> PhoneToWebSockets = new Dictionary<string, List<WebSocket>>();

        public PhoneToSocketConnectionService(ILogger<PhoneToSocketConnectionService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Метод обеспечивает связывание WebSocket-соединений с конкретными
        /// пользователями по их номерам телефонов. Если это первое соединение пользователя, 
        /// создается новый список для хранения всех его соединений. Последующие соединения 
        /// добавляются в список пользователя в соответствии с его номером телефона
        /// </summary>
        /// <param name="webSocket"></param>
        public Dictionary<string, List<WebSocket>> AddWebSocketToClient(WebSocket webSocket, string senderPhoneNumber)
        {
            Locker.EnterWriteLock();
            try
            {
                string phoneNumber = senderPhoneNumber;
                _logger.LogInformation($"Добавление веб-сокета для номера телефона: {phoneNumber} \n");
                if (!PhoneToWebSockets.ContainsKey(phoneNumber))
                {
                    _logger.LogInformation($"Создание нового списка для телефонного номера: {phoneNumber} \n");

                    // Если первое соединение - создаем список для хранения веб-сокет соединений.
                    PhoneToWebSockets[phoneNumber] = new List<WebSocket>();
                }
                // Добавляем новое соединение WebSocket к списку соединений пользователя
                PhoneToWebSockets[phoneNumber].Add(webSocket);

                _logger.LogInformation($"Добавлен веб-сокет для номера телефона: {phoneNumber} \n");

                return PhoneToWebSockets;
            }

            finally
            {
                // Гарантируется, что вызываемый объект выходит из режима записи
                Locker.ExitWriteLock();
            }
        }
        public async Task CloseWebSocketConnectionAsync(WebSocket webSocket, string senderPhoneNumber)
        {
            try
            {
                string phoneNumber = senderPhoneNumber;
                if (PhoneToWebSockets.ContainsKey(phoneNumber))
                {
                    await webSocket.CloseOutputAsync(
                        closeStatus: WebSocketCloseStatus.NormalClosure,
                        statusDescription: webSocket.CloseStatusDescription,
                        cancellationToken: CancellationToken.None);

                    _logger.Log(LogLevel.Information, $"Соединение по номеру: {phoneNumber} закрыто. \n");

                    foreach (string item in PhoneToWebSockets.Keys.ToList())
                    {
                        if (item.Equals(phoneNumber))
                        {
                            PhoneToWebSockets.Remove(item);
                        }
                    }
                    _logger.Log(LogLevel.Information, "Соединение удалено из словаря. \n");
                }
                else
                {
                    _logger.Log(LogLevel.Warning, $"Соединения по номеру {phoneNumber} отсутствуют. \n");
                }
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"Ошибка сервера: {ex.Message} \n");
                throw;
            }

        }
    }
}