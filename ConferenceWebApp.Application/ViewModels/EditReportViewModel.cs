using ConferenceWebApp.Application.DTOs;
using ConferenceWebApp.Application.DTOs.PersonalAccountDTOs;

namespace ConferenceWebApp.Application.ViewModels;

public class EditReportViewModel : IUserProfileViewModel
{
    public UserProfileDTO UserProfile { get; set; } = null!;
    public EditReportDTO Report { get; set; } = new();
}
