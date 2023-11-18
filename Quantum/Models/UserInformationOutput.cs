namespace Quantum.Models
{
    public class UserInfoOutput
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime RegistrationDate { get; set; }
    }
}
