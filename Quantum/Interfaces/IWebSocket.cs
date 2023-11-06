using System.Net.WebSockets;

namespace Quantum.Interfaces
{
    public interface IWebSocket
    {
        public Task HandleWebSocketRequestAsync(WebSocket webSocket);
        public Task EchoAsync(WebSocket webSocket);
    }
}
