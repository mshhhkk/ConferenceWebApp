using ConferenceWebApp.Application.DTOs.Admin;
using ConferenceWebApp.Application.DTOs.PersonalAccountDTOs;

using System.Threading.Tasks;

namespace ConferenceWebApp.Application.Interfaces.Services;

public interface IUserProfileService
{
    Task<Result<UserProfileDTO>> GetByUserIdAsync(Guid userId);
    Task<string> GetUserNameByEmailAsync(string email);

    Task<Result<List<AdminUserProfileDTO>>> GetAllAsync();

    Task<Result<AdminEditUserDTO>> AdminGetByUserIdAsync(Guid userId);
}
