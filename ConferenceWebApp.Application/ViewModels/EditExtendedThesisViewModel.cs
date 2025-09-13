using ConferenceWebApp.Application.DTOs.PersonalAccountDTOs;
using ConferenceWebApp.Application.DTOs.ReportsDTOs;

namespace ConferenceWebApp.Application.ViewModels;

public class EditExtendedThesisViewModel : IUserProfileViewModel
{
    public UserProfileDTO UserProfile { get; set; } = null!;
    public EditExtendedThesisDTO Thesis { get; set; } = new();
}
