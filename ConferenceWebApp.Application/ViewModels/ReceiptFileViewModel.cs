using ConferenceWebApp.Application.DTOs.PersonalAccountDTOs;

namespace ConferenceWebApp.Application.ViewModels;

public class ReceiptFileViewModel : IUserProfileViewModel
{
    public UserProfileDTO UserProfile { get; set; } = null!;
    public ReceiptFileDTO? ReceiptFile { get; set; }
}
