using System.ComponentModel.DataAnnotations;
using ConferenceWebApp.Domain.Entities;

namespace ConferenceWebApp.Application.DTOs.AuthDTOs
{
    public class RegisterDTO
    {
        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Некорректный формат email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Пароль обязателен")]
        [MinLength(8, ErrorMessage = "Пароль должен содержать минимум 8 символов")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Подтверждение пароля обязательно")]
        [Compare(nameof(Password), ErrorMessage = "Пароли не совпадают")]
        [DataType(DataType.Password)]
        public string PasswordRepeat { get; set; } = string.Empty;

        public static RegisterDTO TransformService(User entity)
        {
            return new RegisterDTO
            {
                Email = entity.Email,
            };
        }
    }
}