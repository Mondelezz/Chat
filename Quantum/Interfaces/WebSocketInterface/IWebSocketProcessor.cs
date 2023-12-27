using System.Buffers;
using System.Net.WebSockets;

namespace Quantum.Interfaces.WebSocketInterface
{
    public interface IWebSocketProcessor
    {
        public bool Receive(WebSocketReceiveResult result, ArraySegment<byte> buffer, out ReadOnlySequence<byte> frame);
    }
}
