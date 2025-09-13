using System.ComponentModel.DataAnnotations;

namespace ConferenceWebApp.Application.DTOs.AuthDTOs
{
    public class LoginDTO
    {
        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Некорректный формат email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Пароль обязателен")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        // Опционально: запомнить меня (для кук)
        public bool RememberMe { get; set; } = false;
    }
}