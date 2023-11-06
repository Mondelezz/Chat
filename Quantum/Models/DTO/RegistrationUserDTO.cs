using System.ComponentModel.DataAnnotations;

namespace Quantum.Models.DTO
{
    public class RegistrationUserDTO
    {
        [Required]
        [MinLength(4), MaxLength(20)]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [Phone(ErrorMessage = "Некорректный формат")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;
        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Пароли не совпадают")]
        [Display(Name = "Confirm password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
