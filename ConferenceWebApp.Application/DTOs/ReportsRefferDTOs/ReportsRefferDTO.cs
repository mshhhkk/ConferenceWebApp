namespace ConferenceWebApp.Application.DTOs.RefferReport;

public class ReportsRefferDTO
{
    public Guid ReportId { get; set; } // ID доклада, который передается
    public Guid NewUserId { get; set; }
}