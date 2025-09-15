using ConferenceWebApp.Application.DTOs.PersonalAccountDTOs;

namespace ConferenceWebApp.Application.Interfaces.Services;

public interface IUserProfileService
{
    Task<Result<UserProfileDTO>> GetByUserIdAsync(Guid userId);
    Task<string> GetUserNameByEmailAsync(string email);
}
