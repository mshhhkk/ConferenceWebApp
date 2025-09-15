using ConferenceWebApp.Application.DTOs.PersonalAccountDTOs;
using ConferenceWebApp.Domain.Enums;

namespace ConferenceWebApp.Application.DTOs.ReportsRefferDTOs;

public class ApprovedReportToRefferalDTO
{
    public Guid UserId { get; set; }
    public Guid ReportId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ExtThesis { get; set; } = string.Empty;

}

public class ApprovedReportsForReferralDTO
{
    public List<ApprovedReportToRefferalDTO> Reports { get; set; } = new();
    public List<ApprovedReportToRefferalDTO> IncomingTransfers { get; set; } = new();
}