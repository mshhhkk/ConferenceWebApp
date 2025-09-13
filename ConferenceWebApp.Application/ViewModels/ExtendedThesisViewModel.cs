using ConferenceWebApp.Domain.Entities;
using ConferenceWebApp.Application.DTOs.PersonalAccountDTOs;
namespace ConferenceWebApp.Application.ViewModels;

public class ExtendedThesisViewModel : IUserProfileViewModel
{
    public UserProfileDTO UserProfile { get; set; } = null!;
    public List<Reports> ReportsWithTheses { get; set; } = new();
    public List<Reports> ReportsWithoutTheses { get; set; }
}
