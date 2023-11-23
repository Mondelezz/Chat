using System.ComponentModel.DataAnnotations;

namespace Quantum.Models.DTO
{
    public class TextMessage
    {
        [Key]
        public int MessageId { get; set; }
        public Guid SenderUserId { get; set; }
        public Guid ReceiverUserId { get; set; }
        public string SenderPhoneNumber { get; set; } = string.Empty;
        public string ReceiverPhoneNumber { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime SentTime { get; set; }

    }
}
