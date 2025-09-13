namespace ConferenceWebApp.Application.DTOs.ReportsDTOs;

public class ReportDTO
{
    public Guid Id { get; set; }
    public string ReportTheme { get; set; } = null!;
    public string Section { get; set; } = null!;
    public string WorkType { get; set; } = null!;
    public DateTime UploadedAt { get; set; }
    public DateTime LastUpdatedAt { get; set; }
    public bool IsApproved { get; set; }
}

public class ReportsListDTO
{
    public string FullName { get; set; } = null!;
    public List<ReportDTO> Reports { get; set; } = new();
}