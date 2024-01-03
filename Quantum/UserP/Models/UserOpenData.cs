using System.ComponentModel.DataAnnotations;

namespace Quantum.UserP.Models
{
    public class UserOpenData
    {
        [MinLength(4), MaxLength(20)]
        public string? UserName { get; set; } = string.Empty;
        [Required]
        [Phone(ErrorMessage = "Некорректный формат")]
        public string PhoneNumber { get; set; } = string.Empty;

    }
}
