using ConferenceWebApp.Application.DTOs.PersonalAccountDTOs;
using ConferenceWebApp.Application.DTOs.ReportsRefferDTOs;

namespace ConferenceWebApp.Application.ViewModels;

public class ApprovedReportsForReferralViewModel : IUserProfileViewModel
{
    public UserProfileDTO UserProfile { get; set; } = null!;
    public List<ApprovedReportToRefferalDTO> Reports { get; set; } = new();
    public List<ApprovedReportToRefferalDTO> IncomingTransfers { get; set; } = new();
}

