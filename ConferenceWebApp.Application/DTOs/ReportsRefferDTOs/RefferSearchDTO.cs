using ConferenceWebApp.Application.DTOs.ReportsRefferDTOs;

namespace ConferenceWebApp.Application.DTOs.RefferReport;

public class RefferSearchDTO
{
    public Guid ReportId { get; set; }
    public string? Query { get; set; }
    public List<UserSearchResultDTO> Users { get; set; } = new();
}