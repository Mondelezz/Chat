namespace Quantum.Models.ViewModels
{
    public class MessageViewModel
    {
        public int MessageId { get; set; }
        public string FromUserName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime SentTime { get; set; }
    }
}
