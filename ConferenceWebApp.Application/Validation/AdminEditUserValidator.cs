using ConferenceWebApp.Application.DTOs.Admin;
using ConferenceWebApp.Application.DTOs.PersonalAccountDTOs;
using ConferenceWebApp.Domain.Enums;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConferenceWebApp.Application.Validation;

public class AdminEditUserValidator : AbstractValidator<AdminEditUserDTO>
{
    public AdminEditUserValidator()
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

      
        RuleFor(x => x.Position)
            .NotNull().WithMessage("Выберите должность.")
            .IsInEnum().WithMessage("Выбранная должность недопустима.");

        RuleFor(x => x.Degree)
            .NotNull().WithMessage("Выберите ученую степень.")
            .IsInEnum().WithMessage("Выбранная ученая степень недопустима.");


        RuleFor(x => x.Status)
          .NotNull().WithMessage("Выберите статус")
          .IsInEnum().WithMessage("Выбранный статус недопустим.");


        RuleFor(x => x.ApprovalStatus)
          .NotNull().WithMessage("Выберите статус одобрения.")
          .IsInEnum().WithMessage("Выбранный  статус одобрения недопустим.");



        RuleFor(x => x.ParticipantType)
          .NotNull().WithMessage("Выберите тип участия.")
          .IsInEnum().WithMessage("Выбранный тип участия недопустим.");
    }


}
