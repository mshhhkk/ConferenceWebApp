using ConferenceWebApp.Application;
using ConferenceWebApp.Application.DTOs.PersonalAccountDTOs;

namespace ConferenceWebApp.Infrastructure.Services.Abstract;

public interface IUserProfileService
{
    Task<Result<UserProfileDTO>> GetByUserIdAsync(Guid userId);
}
