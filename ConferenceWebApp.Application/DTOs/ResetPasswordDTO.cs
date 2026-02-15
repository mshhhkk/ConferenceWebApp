using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
namespace ConferenceWebApp.Application.DTOs;


public class PasswordRecoveryDTO
{
    [Required, EmailAddress, Display(Name = "Email")]
    public string Email { get; set; } = default!;
}

public class ResetPasswordDTO
{
    [Required]
    public string UserId { get; set; } = default!;
    [Required]
    public string Token { get; set; } = default!;

    [Required, MinLength(8)]
    [Display(Name = "Новый пароль")]
    public string NewPassword { get; set; } = default!;

    [Required, Compare(nameof(NewPassword), ErrorMessage = "Пароли не совпадают")]
    [Display(Name = "Повторите новый пароль")]
    public string NewPasswordRepeat { get; set; } = default!;
}
