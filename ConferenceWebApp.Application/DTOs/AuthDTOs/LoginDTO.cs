using System.ComponentModel.DataAnnotations;

namespace ConferenceWebApp.Application.DTOs.AuthDTOs;

public class LoginDTO
{
    [Required(ErrorMessage = "Email обязателен")]
    [EmailAddress(ErrorMessage = "Некорректный формат email")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Пароль обязателен")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}