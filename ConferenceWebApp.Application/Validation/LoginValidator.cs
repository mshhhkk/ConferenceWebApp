using ConferenceWebApp.Application.DTOs.AuthDTOs;
using FluentValidation;
using System;

public class LoginValidator : AbstractValidator<LoginDTO>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Email обязателен")
            .MaximumLength(256).WithMessage("Слишком длинный email")
            .EmailAddress().WithMessage("Некорректный формат email");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Пароль обязателен")
            .MinimumLength(8).WithMessage("Минимум 8 символов");
    }
}

