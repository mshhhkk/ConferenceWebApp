using FluentValidation;
using ConferenceWebApp.Application.DTOs.AuthDTOs;

public class RegisterValidator : AbstractValidator<RegisterDTO>
{
    public RegisterValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email обязателен")
            .EmailAddress().WithMessage("Некорректный формат email");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Пароль обязателен")
            .MinimumLength(8).WithMessage("Минимум 8 символов")
             .Matches(@"\p{Ll}").WithMessage("Нужна хотя бы одна строчная буква")
             .Matches(@"\p{Lu}").WithMessage("Нужна хотя бы одна заглавная буква")
            .Matches(@"\d").WithMessage("Нужна хотя бы одна цифра")
            .Matches(@"[^\p{L}\p{Nd}]").WithMessage("Нужен хотя бы один спец-символ");


        RuleFor(x => x.PasswordRepeat)
            .Equal(x => x.Password).WithMessage("Пароли не совпадают");
    }
}
