using System.Net.WebSockets;

namespace Quantum.Interfaces
{
    public interface IWebSocket
    {
        public Task WebSocketRequestAsync(WebSocket webSocket);
        public Task EchoAsync(WebSocket webSocket);
    }
}
