using System.ComponentModel.DataAnnotations;

namespace Quantum.Models.DTO
{
    public class UserDTO
    {
        [Required]
        [MinLength(4), MaxLength(20)]
        public string UserName { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Некорректный формат")]
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
