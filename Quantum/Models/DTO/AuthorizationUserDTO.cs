using System.ComponentModel.DataAnnotations;

namespace Quantum.Models.DTO
{
    public class AuthorizationUserDTO
    {
        [Required]
        [Phone(ErrorMessage = "Некорректный формат")]
        public string PhoneNumber { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
