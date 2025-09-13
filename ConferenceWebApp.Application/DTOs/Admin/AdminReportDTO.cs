namespace ConferenceWebApp.Application.DTOs.Admin;

public class AdminReportDTO
{
    public Guid ReportId { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsApproved { get; set; }
    public string? FilePath { get; set; }
    public string AuthorFullName { get; set; } = string.Empty;
    public string AuthorEmail { get; set; } = string.Empty;
}