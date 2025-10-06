using ConferenceWebApp.Application.DTOs.PersonalAccountDTOs;
using ConferenceWebApp.Application.DTOs.ReportsDTOs;

namespace ConferenceWebApp.ViewModels;

public class EditReportViewModel : IUserProfileViewModel
{
    public UserProfileDTO UserProfile { get; set; } = null!;
    public EditReportDTO Report { get; set; } = new();
}
