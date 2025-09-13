using System.ComponentModel.DataAnnotations;
using ConferenceWebApp.Domain.Enums;
using Microsoft.AspNetCore.Http;

internal static class Invalid
{
    public const string InvalidString = "ABOBUS";
}

namespace ConferenceWebApp.Application.DTOs.ReportsDTOs
{
    public class AddReportDTO
    {
        [Required(ErrorMessage = "Введите тему доклада")]
        public string ReportTheme { get; set; } = Invalid.InvalidString;

        [Required(ErrorMessage = "Выберите секцию")]
        public SectionTopic Section { get; set; }

        [Required(ErrorMessage = "Укажите характер работы")]
        public WorkType WorkType { get; set; }

        [Required(ErrorMessage = "Загрузите файл")]
        public IFormFile File { get; set; }
    }
}