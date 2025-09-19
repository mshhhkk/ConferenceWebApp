using ConferenceWebApp.Application.DTOs.Admin;

public class AdminFilteredReportsListDTO
{
    public List<AdminReportDTO> PendingReports { get; set; } = new();
    public List<AdminReportDTO> ApprovedReports { get; set; } = new();
    public List<AdminReportDTO> RejectedReports { get; set; } = new();
    public List<AdminReportDTO> PendingExtendedTheses { get; set; } = new();
    public List<AdminReportDTO> ApprovedExtendedTheses { get; set; } = new();
    public List<AdminReportDTO> RejectedExtendedTheses { get; set; } = new();
    public string SearchQuery { get; set; } = string.Empty;
    public string Organization { get; set; } = string.Empty;  
    public string SortOrder { get; set; } = "Organization";  
}
