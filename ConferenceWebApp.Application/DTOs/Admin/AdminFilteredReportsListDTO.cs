namespace ConferenceWebApp.Application.DTOs.Admin;

public class AdminFilteredReportsListDTO
{
    public List<AdminReportDTO> ApprovedReports { get; set; } = new();
    public List<AdminReportDTO> PendingReports { get; set; } = new();
    public string SearchQuery { get; set; } = string.Empty;
}