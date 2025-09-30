using ConferenceWebApp.Application.DTOs.PersonalAccountDTOs;
using ConferenceWebApp.Application.DTOs.ReportsDTOs;
using ConferenceWebApp.Domain.Entities;
namespace ConferenceWebApp.ViewModels;

public class ExtendedThesisViewModel : IUserProfileViewModel
{
    public UserProfileDTO UserProfile { get; set; } = null!;
    public List<ReportDTO> ReportsWithTheses { get; set; } = new();
    public List<ReportDTO> ReportsWithoutTheses { get; set; }
}
