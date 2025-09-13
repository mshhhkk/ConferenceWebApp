using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ConferenceWebApp.Application.DTOs.PersonalAccountDTOs;

public class EditUserDTO
{
    [Display(Name = "Имя")]
    public string? FirstName { get; set; } = string.Empty;

    [Display(Name = "Фамилия")]
    public string? LastName { get; set; } = string.Empty;

    [Display(Name = "Отчество")]
    public string? MiddleName { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Введите корректный номер телефона")]
    [Display(Name = "Номер телефона")]
    public string? PhoneNumber { get; set; } = string.Empty;

    [Display(Name = "Дата рождения")]
    [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
    public DateOnly? BirthDate { get; set; }

    [Display(Name = "Организация")]
    public string? Organization { get; set; } = string.Empty;

    [Display(Name = "Специализация")]
    public string? Specialization { get; set; } = string.Empty;

    public string? PhotoUrl { get; set; }

    // Новое свойство для загрузки фотографии, необязательное
    [Display(Name = "Новое фото")]
    public IFormFile? Photo { get; set; }

    public bool RemovePhoto { get; set; } = false;

    public bool IsRegisteredForConference =>
        !string.IsNullOrEmpty(FirstName) &&
        !string.IsNullOrEmpty(LastName) &&
        !string.IsNullOrEmpty(MiddleName) &&
        !string.IsNullOrEmpty(Organization) &&
        !string.IsNullOrEmpty(Specialization) &&
        !string.IsNullOrEmpty(PhoneNumber);
}