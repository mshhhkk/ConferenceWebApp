using ConferenceWebApp.Application.DTOs.PersonalAccountDTOs;
using ConferenceWebApp.Domain.Entities;
namespace ConferenceWebApp.ViewModels;

public class ExtendedThesisViewModel : IUserProfileViewModel
{
    public UserProfileDTO UserProfile { get; set; } = null!;
    public List<Reports> ReportsWithTheses { get; set; } = new();
    public List<Reports> ReportsWithoutTheses { get; set; }
}
