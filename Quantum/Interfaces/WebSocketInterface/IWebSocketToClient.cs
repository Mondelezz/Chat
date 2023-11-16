using System.Net.WebSockets;

namespace Quantum.Interfaces.WebSocketInterface
{
    public interface IWebSocketToClient
    {
        public Dictionary<string, List<WebSocket>> AddWebSocketToClient(WebSocket webSocket, string senderPhoneNumber);
        public Task CloseWebSocketConnection(WebSocket webSocket, string senderPhoneNumber);
    }
}
