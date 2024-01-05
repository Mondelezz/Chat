using Quantum.GroupFolder.Models;
using System.ComponentModel.DataAnnotations;

namespace Quantum.UserP.Models
{
    /// <summary>
    /// Модель пользователя
    /// </summary>
    public class User
    {
        public Guid UserId { get; set; }

        [MinLength(4), MaxLength(20)]
        public string UserName { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Некорректный формат")]
        public string PhoneNumber { get; set; } = string.Empty;
        public string HashPassword { get; set; } = string.Empty;
        public ICollection<GroupUserRole> Roles { get; set; } = new List<GroupUserRole>();
        public ICollection<UserFriends> Friend { get; set; } = new List<UserFriends>();
        public ICollection<UserGroups> Groups { get; set; } = new List<UserGroups>();
        public DateTime RegistrationDate { get; set; }
    }

}
