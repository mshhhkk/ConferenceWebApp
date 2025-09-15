using ConferenceWebApp.Application.DTOs;
using ConferenceWebApp.Application.DTOs.PersonalAccountDTOs;

namespace ConferenceWebApp.Application.Interfaces.Services;

public interface IPersonalAccountService
{
    Task<Result<EditUserDTO>> GetProfileToEditByUserIdAsync(Guid userId);

    Task<Result> UpdateProfileAsync(Guid userId, EditUserDTO dto);

    Task<Result<InvitationDTO>> GenerateInvitationAsync(Guid userId);
}