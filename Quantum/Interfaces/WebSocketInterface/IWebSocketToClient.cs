using System.Net.WebSockets;

namespace Quantum.Interfaces.WebSocketInterface
{
    public interface IWebSocketToClient
    {
        public Dictionary<string, List<WebSocket>> AddWebSocketToClient(WebSocket webSocket, string senderPhoneNumber);
    }
}
