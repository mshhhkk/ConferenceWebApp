namespace ConferenceWebApp.Application.DTOs.ReportsRefferDTOs;

public class ReceivedReportTransferRequestDTO
{
    public Guid RequestId { get; set; }
    public Guid ReportId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ExtThesis { get; set; } = string.Empty;
    public Guid FromUserId { get; set; }
    public string FromUserFullName { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
    public bool IsConfirmed { get; set; } = false;
}
