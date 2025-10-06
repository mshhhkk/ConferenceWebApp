using ConferenceWebApp.Domain.Enums;
using Microsoft.AspNetCore.Http;



namespace ConferenceWebApp.Application.DTOs.ReportsDTOs;

public class AddReportDTO
{
    public string? ReportTheme { get; set; }
    public SectionTopic Section { get; set; }

    public WorkType WorkType { get; set; }

    public IFormFile File { get; set; } = default!;

}
