using System.ComponentModel.DataAnnotations;

namespace ConferenceWebApp.Application.DTOs.PersonalAccountDTOs;

public class ChangePasswordDTO
{
    [Required(ErrorMessage = "Введите текущий пароль.")]
    [DataType(DataType.Password)]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите новый пароль.")]
    [DataType(DataType.Password)]
    [MinLength(6, ErrorMessage = "Пароль должен содержать не менее 6 символов.")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Подтвердите новый пароль.")]
    [DataType(DataType.Password)]
    [Compare("NewPassword", ErrorMessage = "Пароли не совпадают.")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}