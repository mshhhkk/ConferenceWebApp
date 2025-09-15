using ConferenceWebApp.Application.DTOs.PersonalAccountDTOs;
using ConferenceWebApp.Application.DTOs.ReportsDTOs;

namespace ConferenceWebApp.ViewModels;

public class UserReportsViewModel : IUserProfileViewModel
{
    public UserProfileDTO UserProfile { get; set; } = null!;
    public List<ReportDTO> Reports { get; set; } = new();

}
