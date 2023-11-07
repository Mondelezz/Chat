﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Quantum.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Text;

namespace Quantum.Services
{
    public class WebSocketServices : IWebSocket, IJwtTokenService
    {
        private readonly ILogger<WebSocketServices> _logger;
        private static readonly ReaderWriterLockSlim Locker = new ReaderWriterLockSlim();
        private static readonly List<WebSocket> Clients = new List<WebSocket>();
        private readonly HttpContextAccessor _httpContextAccessor;
        public WebSocketServices(ILogger<WebSocketServices> logger, HttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetPhoneNumberFromJwtToken()
        {
            try
            {
                IHeaderDictionary requestHeaders = _httpContextAccessor.HttpContext?.Request.Headers;
                if (requestHeaders != null && requestHeaders.TryGetValue("Authorization", out var authHeaderValue))
                {
                    string jwtToken = authHeaderValue.ToString().Replace("Bearer ", string.Empty);
                    if (!string.IsNullOrEmpty(jwtToken))
                    {
                        JwtSecurityTokenHandler jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
                        JwtSecurityToken jwtSecurityToken = jwtSecurityTokenHandler.ReadJwtToken(jwtToken);
                        Claim? phoneNumberClaim = jwtSecurityToken?.Claims.FirstOrDefault(claim => claim.Type == "PhoneNumber");
                        if (phoneNumberClaim != null)
                        {
                            _logger.LogInformation($"Полученный номер телефона: {phoneNumberClaim.Value}");

                            return phoneNumberClaim.Value;
                        }
                        else
                        {
                            _logger.LogWarning("Не удалось прочитать номер телефона из токена");
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Токен либо отсутствует, либо имеет неверный формат");
                    }
                }
                else
                {
                    _logger.LogWarning("HttpContext или заголовки запроса недоступны");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при извлечении номера телефона из JWT токена");
            }
            return string.Empty;
        }


        /// <summary>
        /// Добавление клиентов
        /// </summary>
        /// <param name="webSocket"></param>
        private void AddWebSocketToClient(WebSocket webSocket)
        {
            Locker.EnterWriteLock();
            try
            {
                // Добавляем сокет клиента в список клиентов
                Clients.Add(webSocket);
            }
            finally
            {
                // Гарантируется, что вызываемый объект выходит из режима записи
                Locker.ExitWriteLock();
            }
        }

        /// <summary>
        /// Удаление клиентов
        /// </summary>
        /// <param name="webSocket"></param>
        private void RemoveWebSocketFromClients(WebSocket webSocket)
        {
            Locker.EnterWriteLock();
            try
            {
                Clients.Remove(webSocket);
            }
            finally
            {
                Locker.ExitWriteLock();
            }
        }

        /// <summary>
        /// Управленние веб-сокет соединениями
        /// </summary>
        public async Task HandleWebSocketRequestAsync(WebSocket webSocket)
        {
            AddWebSocketToClient(webSocket);           
            try
            {
                while (webSocket.State == WebSocketState.Open)
                {
                    ArraySegment<byte> buffers = new ArraySegment<byte>(new byte[4096]);
                    /// <summary>
                    /// result содержит информацию чтения данных из веб-сокета.
                    /// </summary>
                    WebSocketReceiveResult result = await webSocket.ReceiveAsync(buffers, CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Text && result.EndOfMessage)
                    {
                        byte[] receivedBytes = buffers.Skip(buffers.Offset).Take(result.Count).ToArray();

                        await BroadcastMessageAsync(receivedBytes);
                    }                  
                }
            }           
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"Исключение: {ex.Message}");
                RemoveWebSocketFromClients(webSocket);
            }
        } 
        
        private async Task BroadcastMessageAsync(byte[] message)
        {
            Locker.EnterWriteLock();
            try
            {
                // Отправить сообщение всем подключенным клиентам
                foreach (var client in Clients)
                {
                    if (client.State == WebSocketState.Open)
                    {
                        ArraySegment<byte> buffer = new ArraySegment<byte>(message);
                        await client.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
                        _logger.Log(LogLevel.Information, "Сообщение было отправлено клиенту");
                    }
                }
            }
            finally
            {
                Locker.ExitWriteLock();
            }
        }
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task SendWebSocketmessageToUser(WebSocket webSocket)
        {
            AddWebSocketToClient(webSocket);
            try
            {
                while (webSocket.State == WebSocketState.Open)
                {
                    ArraySegment<byte> buffers = new ArraySegment<byte>(new byte[4096]);

                    WebSocketReceiveResult result = await webSocket.ReceiveAsync(buffers, CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Text && result.EndOfMessage)
                    {
                        byte[] resceivedBytes = buffers.Skip(buffers.Offset).Take(result.Count).ToArray();

                        Locker.EnterWriteLock();
                        try
                        {

                        }
                        catch (Exception)
                        {

                            throw;
                        }
                    }
                    
                }
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Warning, $"Исключение: {ex.Message}");
                RemoveWebSocketFromClients(webSocket);
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
    }
}
