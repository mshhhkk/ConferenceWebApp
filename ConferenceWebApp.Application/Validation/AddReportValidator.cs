
using ConferenceWebApp.Application.DTOs.ReportsDTOs;
using FluentValidation;
using Microsoft.AspNetCore.Http;


namespace ConferenceWebApp.Application.Validation;

public class AddReportValidator : AbstractValidator<AddReportDTO>
{
    public AddReportValidator()
    {
  
        RuleFor(x => x.ReportTheme)
            .NotEmpty().WithMessage("Тема отчёта обязательна для заполнения")
            .Length(5, 200).WithMessage("Тема отчёта должна содержать от 5 до 200 символов");

        RuleFor(x => x.Section)
            .IsInEnum().WithMessage("Необходимо выбрать секцию");

        RuleFor(x => x.WorkType)
            .IsInEnum().WithMessage("Необходимо указать тип работы");

        RuleFor(x => x.File)
            .Must(BeAValidFile).WithMessage("Допустимы только файлы форматов DOC и DOCX")
            .Must(BeAValidFileSize).WithMessage("Размер файла не должен превышать 50 МБ");
    }
    private bool BeAValidFile(IFormFile file)
    {
        if (file == null) return true; 

        var allowedExtensions = new[] { ".doc", ".docx" };
        var extension = Path.GetExtension(file.FileName).ToLower();
        return allowedExtensions.Contains(extension);
    }

 
    private bool BeAValidFileSize(IFormFile file)
    {
        if (file == null) return true; 

        return file.Length <= 50 * 1024 * 1024; 
    }
}
