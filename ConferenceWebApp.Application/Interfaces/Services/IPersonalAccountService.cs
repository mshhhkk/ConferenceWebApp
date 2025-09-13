using ConferenceWebApp.Application;
using ConferenceWebApp.Application.DTOs.PersonalAccountDTOs;
using ConferenceWebApp.Application.ViewModels;
using ConferenceWebApp.Application.DTOs;

namespace ConferenceWebApp.Infrastructure.Services.Abstract;

public interface IPersonalAccountService
{
    Task<Result<UserProfileViewModel>> GetUserProfileAsync(Guid userId);

    Task<Result<EditUserDTO>> GetEditableProfileAsync(Guid userId);

    Task<Result> UpdateProfileAsync(Guid userId, EditUserDTO dto);

    Task<Result<InvitationDTO>> GenerateInvitationAsync(Guid userId);
}