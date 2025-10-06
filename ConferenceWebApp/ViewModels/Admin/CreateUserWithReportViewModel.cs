using ConferenceWebApp.Application.DTOs.PersonalAccountDTOs;
using ConferenceWebApp.Application.DTOs.ReportsDTOs;

namespace ConferenceWebApp.Application.DTOs.Admin;

public class CreateUserWithReportViewModel
{
    public string Email { get; set; } = string.Empty;
    public AddReportDTO Report { get; set; } = new();
    public EditUserDTO UserProfile { get; set; } = new();
}
