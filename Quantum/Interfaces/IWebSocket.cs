using System.Net.WebSockets;

namespace Quantum.Interfaces
{
    public interface IWebSocket
    {
        //public Task HandleWebSocketRequestAsync(WebSocket webSocket);
       // public Task HandleNewWebSocketConnection(WebSocket webSocket, string message, string receiverPhoneNumber);
        public Task EchoAsync(WebSocket webSocket);
        public void AddWebSocketToClient(WebSocket webSocket, string token);
        public Task SendMessageToUser(string senderPhoneNumber, string receiverPhoneNumber, byte[] messageBytes);
    }
}
