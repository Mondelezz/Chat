using Quantum.UserP.Models;
using System.Net.WebSockets;

namespace Quantum.Interfaces.WebSocketInterface
{
    public interface IWebSocket
    {
        public Task EchoAsync(WebSocket webSocket);
        public Task<bool> SendMessageToUserAsync(string senderPhoneNumber, string receiverPhoneNumber, byte[] messageBytes, Dictionary<string, List<WebSocket>> phoneToWebSockets);
        public Task SaveMessageInDataBaseAsync(User userSender, User userReceiver, byte[] messageBytes);
    }
}
