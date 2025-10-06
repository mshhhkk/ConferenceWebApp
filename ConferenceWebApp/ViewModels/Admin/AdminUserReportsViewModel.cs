using ConferenceWebApp.Application.DTOs.ReportsDTOs;

namespace ConferenceWebApp.ViewModels.Admin;

public class AdminUserReportsViewModel
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;

    public List<ReportDTO> Reports { get; set; } = new List<ReportDTO>();
}
