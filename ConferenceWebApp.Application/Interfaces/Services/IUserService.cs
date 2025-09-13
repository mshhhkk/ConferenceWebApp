using ConferenceWebApp.Application.DTOs.PersonalAccountDTOs;

namespace ConferenceWebApp.Infrastructure.Services.Abstract;

public interface IUserService
{
    Task<UserProfileDTO> GetUserProfileAsync(Guid userId);
}
