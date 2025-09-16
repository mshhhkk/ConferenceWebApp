using ConferenceWebApp.Application.DTOs.PersonalAccountDTOs;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using System;
using System.Text.RegularExpressions;

public class EditUserProfileValidator : AbstractValidator<EditUserDTO>
{
    public EditUserProfileValidator()
    {
        RuleFor(x => x.FirstName)
            .Matches(@"^[A-Za-zА-Яа-яЁё]+$").WithMessage("Имя может содержать только буквы.")
            .NotEmpty().WithMessage("Имя обязательно для заполнения.")
            .MaximumLength(50).WithMessage("Имя не может быть длиннее 50 символов.");

        RuleFor(x => x.LastName)
            .Matches(@"^[A-Za-zА-Яа-яЁё]+$").WithMessage("Фамилия может содержать только буквы.")
            .NotEmpty().WithMessage("Фамилия обязательна для заполнения.")
            .MaximumLength(50).WithMessage("Фамилия не может быть длиннее 50 символов.");

        RuleFor(x => x.MiddleName)
            .Matches(@"^[A-Za-zА-Яа-яЁё]+$").WithMessage("Отчество может содержать только буквы.")
            .MaximumLength(50).WithMessage("Отчество не может быть длиннее 50 символов.");

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+375\d{9}$").WithMessage("Номер телефона должен быть в формате +375xxxxxxxxx.")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

        RuleFor(x => x.BirthDate)
            .NotNull().WithMessage("Дата рождения обязательна для заполнения.")
            .LessThan(DateOnly.FromDateTime(DateTime.Now.AddYears(-15))).WithMessage("Возраст должен быть не менее 15 лет.")
            .GreaterThan(DateOnly.FromDateTime(DateTime.Now.AddYears(-100))).WithMessage("Возраст не может быть старше 100 лет.");

        RuleFor(x => x.Organization)
            .NotEmpty().WithMessage("Организация обязательна для заполнения.")
            .MaximumLength(100).WithMessage("Название организации не может быть длиннее 100 символов.");

        RuleFor(x => x.Specialization)
            .NotEmpty().WithMessage("Специализация обязательна для заполнения.")
            .MaximumLength(100).WithMessage("Специализация не может быть длиннее 100 символов.");

        RuleFor(x => x.Photo)
            .Must(photo => photo == null || photo.Length < 10 * 1024 * 1024)
            .WithMessage("Фото не может быть больше 10 МБ.")
            .When(x => x.Photo != null);


        RuleFor(x => x.Position)
            .NotNull().WithMessage("Выберите должность.")
            .IsInEnum().WithMessage("Выбранная должность недопустима.");

        RuleFor(x => x.Degree)
            .NotNull().WithMessage("Выберите ученую степень.")
            .IsInEnum().WithMessage("Выбранная ученая степень недопустима.");

    }
}
