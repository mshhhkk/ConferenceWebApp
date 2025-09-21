using ConferenceWebApp.Application.DTOs.ReportsDTOs;

namespace ConferenceWebApp.ViewModels.Admin;

public class AdminUserReportsViewModel
{
    public Guid UserId { get; set; }
    public string Email { get; set; }

    public List<ReportDTO> Reports {get;set;}
}
