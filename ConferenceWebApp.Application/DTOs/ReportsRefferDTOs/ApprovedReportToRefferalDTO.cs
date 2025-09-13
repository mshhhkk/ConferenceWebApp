namespace ConferenceWebApp.Application.DTOs.ReportsRefferDTOs;

public class ApprovedReportToRefferalDTO
{
    public Guid UserId { get; set; }
    public Guid ReportId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ExtThesis { get; set; } = string.Empty;
    public bool IsTransferRequested { get; set; } = true;
    public bool IsTransferConfirmed { get; set; }
}
