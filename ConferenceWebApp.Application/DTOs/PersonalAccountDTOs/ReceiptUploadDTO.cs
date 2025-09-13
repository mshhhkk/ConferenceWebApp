using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ConferenceWebApp.Application.DTOs.PersonalAccountDTOs;

public class ReceiptUploadDTO
{
    [Required(ErrorMessage = "Файл обязателен для загрузки.")]
    [AllowedExtensions(new[] { ".jpg", ".jpeg", ".png", ".pdf" }, ErrorMessage = "Разрешены только файлы JPG, PNG и PDF.")]
    public IFormFile Receipt { get; set; } = null!;
}

public class AllowedExtensionsAttribute : ValidationAttribute
{
    private readonly string[] _extensions;

    public AllowedExtensionsAttribute(string[] extensions)
    {
        _extensions = extensions;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!_extensions.Contains(extension))
            {
                return new ValidationResult(ErrorMessage ?? "Недопустимый формат файла.");
            }
        }
        return ValidationResult.Success;
    }
}