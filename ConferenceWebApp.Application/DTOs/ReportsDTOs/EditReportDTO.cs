using System.ComponentModel.DataAnnotations;
using ConferenceWebApp.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace ConferenceWebApp.Application.DTOs.ReportsDTOs
{
    public class EditReportDTO
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Тема отчёта обязательна для заполнения")]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "Тема отчёта должна содержать от 5 до 200 символов")]
        [Display(Name = "Тема научного отчёта")]
        public string ReportTheme { get; set; } = string.Empty;

        [Required(ErrorMessage = "Необходимо выбрать секцию")]
        [Display(Name = "Научная секция")]
        public SectionTopic Section { get; set; }

        [Required(ErrorMessage = "Необходимо указать тип работы")]
        [Display(Name = "Тип научной работы")]
        public WorkType WorkType { get; set; }

        [DataType(DataType.Upload)]
        public IFormFile? File { get; set; }

        public string? CurrentFileName { get; set; }
        public string? CurrentFilePath { get; set; }
        public DateTime? CurrentFileUploadDate { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (File != null)
            {
                var allowedExtensions = new[] { ".doc", ".docx" };
                var extension = Path.GetExtension(File.FileName).ToLower();

                if (!allowedExtensions.Contains(extension))
                {
                    yield return new ValidationResult(
                        "Допустимы только файлы форматов DOC и DOCX",
                        new[] { nameof(File) });
                }

                if (File.Length > 50 * 1024 * 1024) //
                {
                    yield return new ValidationResult(
                        "Размер файла не должен превышать 50 МБ",
                        new[] { nameof(File) });
                }
            }
        }
    }
}