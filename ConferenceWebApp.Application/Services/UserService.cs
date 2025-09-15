using ConferenceWebApp.Application.DTOs.PersonalAccountDTOs;
using ConferenceWebApp.Infrastructure.Services.Abstract;
using ConferenceWebApp.Application.Interfaces.Repositories;
using ConferenceWebApp.Domain.Entities;
using ConferenceWebApp.Domain.Enums;
using ConferenceWebApp.Application;
namespace ConferenceWebApp.Infrastructure.Services.Realization;

public class UserService : IUserProfileService
{
    private readonly IUserProfileRepository _userProfileRepository;
    public UserService(IUserProfileRepository userProfileRepository)
    {
        _userProfileRepository = userProfileRepository;
    }
    public async Task<Result<UserProfileDTO>> GetByUserIdAsync(Guid userId)
    {
        
        var userProfile = await _userProfileRepository.GetByUserIdAsync(userId);
        if (userProfile==null)
        {
            return Result<UserProfileDTO>.Failure("Профиль не найден");
        }

        var dto = new UserProfileDTO
        {
            FullName = $"{userProfile.LastName} {userProfile.FirstName} {userProfile.MiddleName}".Trim(),
            Email = userProfile.User?.Email ?? string.Empty,
            PhoneNumber = userProfile.PhoneNumber ?? string.Empty,
            BirthDate = userProfile.BirthDate,
            Organization = userProfile.Organization ?? string.Empty,
            Specialization = userProfile.Specialization ?? string.Empty,
            PhotoUrl = userProfile.PhotoUrl,
            ParticipantType = userProfile.ParticipantType,
            Status = userProfile.Status,
            IsApprovedAnyReports = (userProfile.ApprovalStatus > UserApprovalStatus.None),
            IsExtendedThesisApproved = (userProfile.ApprovalStatus == UserApprovalStatus.ExtendedThesisApproved)
        };

        return Result<UserProfileDTO>.Success(dto);

    }
}
