using ConferenceWebApp.Application;
using ConferenceWebApp.Application.DTOs.Admin;
using ConferenceWebApp.Application.DTOs.PersonalAccountDTOs;
using ConferenceWebApp.Application.Interfaces.Repositories;
using ConferenceWebApp.Application.Interfaces.Services;
using ConferenceWebApp.Domain.Entities;
using ConferenceWebApp.Domain.Enums;
namespace ConferenceWebApp.Infrastructure.Services.Realization;

public class UserProfileService : IUserProfileService
{
    private readonly IUserProfileRepository _userProfileRepository;
    public UserProfileService(IUserProfileRepository userProfileRepository)
    {
        _userProfileRepository = userProfileRepository;
    }

    public async Task<Result<UserProfileDTO>> GetByUserIdAsync(Guid userId)
    {

        var userProfile = await _userProfileRepository.GetByUserIdAsync(userId);
        if (userProfile == null)
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
    public async Task<Result<AdminEditUserDTO>> AdminGetByUserIdAsync(Guid userId)
    {
        var userProfile = await _userProfileRepository.GetByUserIdAsync(userId);
        if (userProfile == null)
        {
            return Result<AdminEditUserDTO>.Failure("Профиль не найден");
        }
       
        var dto = new AdminEditUserDTO
        {
            LastName = userProfile.LastName,
            FirstName = userProfile.FirstName,
            MiddleName = userProfile.MiddleName,
            PhoneNumber = userProfile.PhoneNumber ?? string.Empty,
            BirthDate = userProfile.BirthDate,
            Organization = userProfile.Organization ?? string.Empty,
            Specialization = userProfile.Specialization ?? string.Empty,
            Status = userProfile.Status,
            Position = userProfile.Position,
            Degree = userProfile.Degree,
            ParticipantType = userProfile.ParticipantType,
            ApprovalStatus = userProfile.ApprovalStatus
        };

        return Result<AdminEditUserDTO>.Success(dto);
    }
    public async Task<string> GetUserNameByEmailAsync(string email)
    {
        var user = await _userProfileRepository.GetUserProfileByEmail(email);
        var username = user.LastName + " " + user.FirstName[0] + ".";
        return username;
    }

    public async Task<Result<List<AdminUserProfileDTO>>> GetAllAsync()
    {
        var users = _userProfileRepository.GetAllList();

        if (users == null || !users.Any())
        {
            return Result<List<AdminUserProfileDTO>>.Failure("Нет пользователей в базе данных.");
        }

        var result = users.Select(user => new AdminUserProfileDTO
        {
            UserId = user.UserId,
            FullName = $"{user.FirstName} {(string.IsNullOrWhiteSpace(user.LastName) ? "" : user.LastName)} {(string.IsNullOrWhiteSpace(user.MiddleName) ? "" : user.MiddleName)}",
            BirthDate = user.BirthDate.ToString(), 
            Organization = user.Organization,
            Specialization = user.Specialization,
            PhoneNumber = user.PhoneNumber,
            Status = user.Status, 
            Position = user.Position, 
            Degree = user.Degree,
            ParticipantType = user.ParticipantType,
            ApprovalStatus = user.ApprovalStatus
        }).ToList();

        return Result<List<AdminUserProfileDTO>>.Success(result);
    }

}
