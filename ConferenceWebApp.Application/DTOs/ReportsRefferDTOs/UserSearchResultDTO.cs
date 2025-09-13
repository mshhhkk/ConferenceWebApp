namespace ConferenceWebApp.Application.DTOs.ReportsRefferDTOs;

public class UserSearchResultDTO
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public Guid ReportId { get; set; }
}