﻿using Quantum.Interfaces.WebSocketInterface;
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
                _logger.LogInformation($"Добавление веб-сокета для номера телефона: {phoneNumber}");
                if (!PhoneToWebSockets.ContainsKey(phoneNumber))
                {
                    _logger.LogInformation($"Создание нового списка для телефонного номера: {phoneNumber}");

                    // Если первое соединение - создаем список для хранения веб-сокет соединений.
                    PhoneToWebSockets[phoneNumber] = new List<WebSocket>();
                }
                // Добавляем новое соединение WebSocket к списку соединений пользователя
                PhoneToWebSockets[phoneNumber].Add(webSocket);

                _logger.LogInformation($"Добавлен веб-сокет для номера телефона: {phoneNumber}");

                return PhoneToWebSockets;
            }

            finally
            {
                // Гарантируется, что вызываемый объект выходит из режима записи
                Locker.ExitWriteLock();
            }
        }       
    }
}