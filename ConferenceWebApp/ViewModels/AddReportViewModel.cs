using ConferenceWebApp.Application.DTOs.PersonalAccountDTOs;
using ConferenceWebApp.Application.DTOs.ReportsDTOs;

namespace ConferenceWebApp.ViewModels;

public class AddReportViewModel : IUserProfileViewModel
{
    public UserProfileDTO UserProfile { get; set; } = null!;
    public AddReportDTO Report { get; set; } = new();
}
