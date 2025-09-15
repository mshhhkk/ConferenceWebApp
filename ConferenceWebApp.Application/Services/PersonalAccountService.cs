using ConferenceWebApp.Domain.Entities;
using ConferenceWebApp.Domain.Enums;
using ConferenceWebApp.Application.DTOs;
using ConferenceWebApp.Application.DTOs.PersonalAccountDTOs;
using ConferenceWebApp.Application.ViewModels;
using ConferenceWebApp.Infrastructure.Services.Abstract;
using ConferenceWebApp.Application;
using ConferenceWebApp.Application.Interfaces.Services;
using ConferenceWebApp.Application.Interfaces.Repositories;

namespace ConferenceWebApp.Infrastructure.Services.Realization;

public class PersonalAccountService : IPersonalAccountService
{
    private const long MaxPhotoSize = 5 * 1024 * 1024;
    private static readonly string[] AllowedPhotoTypes = { "image/jpeg", "image/png", "image/gif" };
    private const string DefaultPhotoPath = "/images/defaultUserPhoto.png";

    private readonly IUserProfileRepository _userProfileRepository;
    private readonly IReportsRepository _reportsRepository;
    private readonly IFileService _fileService;

    public PersonalAccountService(
        IUserProfileRepository userProfileRepository,
        IReportsRepository reportsRepository,
        IFileService fileService)
    {
        _userProfileRepository = userProfileRepository;
        _reportsRepository = reportsRepository;
        _fileService = fileService;
    }

    public async Task<Result<UserProfileViewModel>> GetUserProfileAsync(Guid userId)
    {
        var userProfile = await _userProfileRepository.GetByUserIdAsync(userId);
        if (userProfile == null)
            return Result<UserProfileViewModel>.Failure("Профиль пользователя не найден");

        var userReports = await _reportsRepository.GetApprovedReportsByUserIdAsync(userId);

        var photoUrl = DefaultPhotoPath;
        if (!string.IsNullOrEmpty(userProfile.PhotoUrl) &&
        !userProfile.PhotoUrl.Equals(DefaultPhotoPath, StringComparison.OrdinalIgnoreCase))
        {
            var meta = await _fileService.TryGetFileMetadataAsync(userProfile.PhotoUrl);
            if (meta?.Exists == true) photoUrl = userProfile.PhotoUrl;
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

        var vm = new UserProfileViewModel
        {
            UserProfile = dto
        };
        return Result<UserProfileViewModel>.Success(vm);
    }

    public async Task<Result<EditUserDTO>> GetEditableProfileAsync(Guid userId)
    {
        var userProfile = await _userProfileRepository.GetByUserIdAsync(userId);
        if (userProfile == null)
            return Result<EditUserDTO>.Failure("Профиль пользователя не найден");

        if (userProfile.ParticipantType == ParticipantType.Speaker)
            return Result<EditUserDTO>.Failure("Пользователь с одобренным докладом не может поменять профиль");

        return Result<EditUserDTO>.Success(new EditUserDTO
        {
            FirstName = userProfile.FirstName,
            LastName = userProfile.LastName,
            MiddleName = userProfile.MiddleName,
            PhoneNumber = userProfile.PhoneNumber,
            BirthDate = userProfile.BirthDate,
            Specialization = userProfile.Specialization,
            Organization = userProfile.Organization,
            PhotoUrl = userProfile.PhotoUrl
        });
    }

    public async Task<Result> UpdateProfileAsync(Guid userId, EditUserDTO dto)
    {
        var userProfile = await _userProfileRepository.GetByUserIdAsync(userId);
        if (userProfile == null)
            return Result.Failure("Профиль пользователя не найден");

        try
        {
            userProfile.FirstName = dto.FirstName;
            userProfile.LastName = dto.LastName;
            userProfile.MiddleName = dto.MiddleName;
            userProfile.PhoneNumber = dto.PhoneNumber;
            userProfile.BirthDate = (DateOnly)dto.BirthDate;
            userProfile.Organization = dto.Organization;
            userProfile.Specialization = dto.Specialization;

            if (dto.RemovePhoto)
            {
                await HandlePhotoRemoval(userProfile);
            }
            else if (dto.Photo != null)
            {
                if (!string.IsNullOrEmpty(userProfile.PhotoUrl) &&
                !userProfile.PhotoUrl.Equals(DefaultPhotoPath, StringComparison.OrdinalIgnoreCase))
                {
                    _fileService.DeleteFile(userProfile.PhotoUrl);
                }

                userProfile.PhotoUrl = await _fileService.UpdateFileAsync(
                    dto.Photo,
                    null,
                    "images",
                    AllowedPhotoTypes,
                    MaxPhotoSize);
            }

            await _userProfileRepository.UpdateAsync(userProfile);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Ошибка при обновлении профиля: {ex.Message}");
        }
    }

    private async Task HandlePhotoRemoval(UserProfile userProfile)
    {
        if (!string.IsNullOrEmpty(userProfile.PhotoUrl) &&
            !userProfile.PhotoUrl.Equals(DefaultPhotoPath, StringComparison.OrdinalIgnoreCase))
        {
            _fileService.DeleteFile(userProfile.PhotoUrl);
        }
        userProfile.PhotoUrl = DefaultPhotoPath;
    }

    public async Task<Result<InvitationDTO>> GenerateInvitationAsync(Guid userId)
    {
        var profile = await _userProfileRepository.GetByUserIdAsync(userId);
        if (profile == null)
            return Result<InvitationDTO>.Failure("Профиль пользователя не найден");

        return Result<InvitationDTO>.Success(new InvitationDTO
        {
            FullName = $"{profile.FirstName} {profile.LastName}",
            ConferenceName = "Международная конференция 2025",
            Organizer = "Организационный комитет"
        });
    }
}