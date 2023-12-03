

using System.ComponentModel.DataAnnotations;

namespace Quantum.Models
{
    public class UserInfoOutput
    {
        [Key]
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime RegistrationDate { get; set; }
        public ICollection<UserFriends> Users { get; set; } = new List<UserFriends>();
    }
}
