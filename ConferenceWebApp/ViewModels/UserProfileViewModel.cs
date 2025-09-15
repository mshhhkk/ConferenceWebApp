using ConferenceWebApp.Application.DTOs.PersonalAccountDTOs;

namespace ConferenceWebApp.ViewModels;

public class UserProfileViewModel : IUserProfileViewModel
{
    public UserProfileDTO UserProfile { get; set; } = null!;
}
