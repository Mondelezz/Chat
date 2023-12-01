using System.ComponentModel.DataAnnotations;

namespace Quantum.Models
{
    public class UsersOpenData
    {
        [MinLength(4), MaxLength(20)]
        public string? UserName { get; set; } = string.Empty;
        [Required]
        [Phone(ErrorMessage = "Некорректный формат")]
        public string PhoneNumber { get; set; } = string.Empty;
        
    }
}
