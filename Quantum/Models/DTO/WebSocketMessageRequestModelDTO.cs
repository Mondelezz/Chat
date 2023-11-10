using System.Net.WebSockets;

namespace Quantum.Models.DTO
{
    public class WebSocketMessageRequestModelDTO
    {       
        public string ReceiverPhoneNumber { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty; 
    }
}
