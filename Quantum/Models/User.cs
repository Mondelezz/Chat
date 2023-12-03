using System.ComponentModel.DataAnnotations;

namespace Quantum.Models
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
        public ICollection<UserFriends> Friends { get; set; } = new List<UserFriends>();
        public DateTime RegistrationDate { get; set; }
    }

}
