using ConferenceWebApp.Application;
using ConferenceWebApp.Application.DTOs;
using ConferenceWebApp.Application.DTOs.Admin;
using ConferenceWebApp.Application.DTOs.PersonalAccountDTOs;
using ConferenceWebApp.Application.Interfaces.Repositories;
using ConferenceWebApp.Application.Interfaces.Services;
using ConferenceWebApp.Domain.Entities;
using ConferenceWebApp.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace ConferenceWebApp.Infrastructure.Services.Realization;

public class PersonalAccountService : IPersonalAccountService
{
    private const long MaxPhotoSize = 5 * 1024 * 1024;
    private static readonly string[] AllowedPhotoTypes = { "image/jpeg", "image/png", "image/gif" };
    private const string DefaultPhotoPath = "/images/user.svg";

    private readonly IUserProfileRepository _userProfileRepository;
    private readonly IReportsRepository _reportsRepository;
    private readonly IFileService _fileService;
    private readonly ILogger<PersonalAccountService> _logger;

    public PersonalAccountService(
        IUserProfileRepository userProfileRepository,
        IReportsRepository reportsRepository,
        IFileService fileService,
        ILogger<PersonalAccountService> logger)
    {
        _userProfileRepository = userProfileRepository;
        _reportsRepository = reportsRepository;
        _fileService = fileService;
        _logger = logger;
    }

    public async Task<Result<EditUserDTO>> GetProfileToEditByUserIdAsync(Guid userId)
    {
        try
        {
            _logger.LogInformation("Запрос профиля для редактирования UserId={UserId}", userId);

            var userProfile = await _userProfileRepository.GetByUserIdAsync(userId);
            if (userProfile == null)
            {
                _logger.LogWarning("Профиль пользователя не найден UserId={UserId}", userId);
                return Result<EditUserDTO>.Failure("Профиль пользователя не найден");
            }

            var dto = new EditUserDTO
            {
                FirstName = userProfile.FirstName,
                LastName = userProfile.LastName,
                MiddleName = userProfile.MiddleName,
                PhoneNumber = userProfile.PhoneNumber,
                BirthDate = userProfile.BirthDate,
                Specialization = userProfile.Specialization,
                Organization = userProfile.Organization,
                PhotoUrl = userProfile.PhotoUrl,
                Degree = userProfile.Degree,
                Position = userProfile.Position
            };

            _logger.LogInformation("Профиль сформирован для редактирования UserId={UserId}", userId);
            return Result<EditUserDTO>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении профиля для редактирования UserId={UserId}", userId);
            return Result<EditUserDTO>.Failure($"Ошибка при получении профиля: {ex.Message}");
        }
    }

    public async Task<Result> UpdateProfileAsync(Guid userId, EditUserDTO dto)
    {
        try
        {
            _logger.LogInformation("Обновление профиля UserId={UserId}", userId);

            var userProfile = await _userProfileRepository.GetByUserIdAsync(userId);
            if (userProfile == null)
            {
                _logger.LogWarning("Профиль не найден при обновлении UserId={UserId}", userId);
                return Result.Failure("Профиль пользователя не найден");
            }

            if (userProfile.PhotoUrl == DefaultPhotoPath && dto.RemovePhoto)
            {
                _logger.LogWarning("Попытка удалить дефолтное фото UserId={UserId}", userId);
                return Result.Failure("Невозможно удалить дефолтное фото.");
            }

            if (userProfile.ParticipantType == ParticipantType.Speaker)
            {
                _logger.LogWarning("Попытка изменить профиль спикера с одобренным докладом UserId={UserId}", userId);
                return Result<EditUserDTO>.Failure("Пользователь с одобренным докладом не может поменять профиль");
            }

            userProfile.FirstName = dto.FirstName;
            userProfile.LastName = dto.LastName;
            userProfile.MiddleName = dto.MiddleName;
            userProfile.PhoneNumber = dto.PhoneNumber;
            userProfile.BirthDate = (DateOnly)dto.BirthDate;
            userProfile.Organization = dto.Organization;
            userProfile.Specialization = dto.Specialization;
            userProfile.Degree = dto.Degree!.Value;
            userProfile.Position = dto.Position!.Value;
            userProfile.Status = ParticipantStatus.ProfileCompleted;

            if (dto.RemovePhoto)
            {
                _logger.LogInformation("Удаление пользовательского фото UserId={UserId}", userId);
                await HandlePhotoRemoval(userProfile);
            }
            else if (dto.Photo != null)
            {
                _logger.LogInformation("Обновление фото профиля UserId={UserId}, ContentType={ContentType}, Size={Size}",
                    userId, dto.Photo.ContentType, dto.Photo.Length);

                if (!string.IsNullOrEmpty(userProfile.PhotoUrl) &&
                    !userProfile.PhotoUrl.Equals(DefaultPhotoPath, StringComparison.OrdinalIgnoreCase))
                {
                    _fileService.DeleteFile(userProfile.PhotoUrl);
                    _logger.LogInformation("Старое фото удалено UserId={UserId}", userId);
                }

                userProfile.PhotoUrl = await _fileService.UpdateFileAsync(
                    dto.Photo,
                    null,
                    "uploads",
                    AllowedPhotoTypes,
                    MaxPhotoSize);

                _logger.LogInformation("Новое фото загружено UserId={UserId}, Path={Path}", userId, userProfile.PhotoUrl);
            }

            await _userProfileRepository.UpdateAsync(userProfile);
            _logger.LogInformation("Профиль обновлён UserId={UserId}", userId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении профиля UserId={UserId}", userId);
            return Result.Failure($"Ошибка при обновлении профиля: {ex.Message}");
        }
    }

    private async Task HandlePhotoRemoval(UserProfile userProfile)
    {
        try
        {
            if (!string.IsNullOrEmpty(userProfile.PhotoUrl) &&
                !userProfile.PhotoUrl.Equals(DefaultPhotoPath, StringComparison.OrdinalIgnoreCase))
            {
                _fileService.DeleteFile(userProfile.PhotoUrl);
                _logger.LogInformation("Удален файл фото {Path} для UserId={UserId}", userProfile.PhotoUrl, userProfile.UserId);
            }
            userProfile.PhotoUrl = DefaultPhotoPath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при удалении фото UserId={UserId}, Path={Path}", userProfile.UserId, userProfile.PhotoUrl);
            throw; 
        }
    }

    public async Task<Result<InvitationDTO>> GenerateInvitationAsync(Guid userId)
    {
        try
        {
            _logger.LogInformation("Генерация приглашения UserId={UserId}", userId);

            var profile = await _userProfileRepository.GetByUserIdAsync(userId);
            if (profile == null)
            {
                _logger.LogWarning("Профиль не найден при генерации приглашения UserId={UserId}", userId);
                return Result<InvitationDTO>.Failure("Профиль пользователя не найден");
            }

            var dto = new InvitationDTO
            {
                FullName = $"{profile.FirstName} {profile.LastName}",
                ConferenceName = "Международная конференция 2025",
                Organizer = "Организационный комитет"
            };

            _logger.LogInformation("Приглашение сформировано UserId={UserId}", userId);
            return Result<InvitationDTO>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при генерации приглашения UserId={UserId}", userId);
            return Result<InvitationDTO>.Failure($"Ошибка при генерации приглашения: {ex.Message}");
        }
    }

    public async Task<Result> AdminUpdateProfileAsync(Guid userId, AdminEditUserDTO dto)
    {
        try
        {
            _logger.LogInformation("Админ-обновление профиля UserId={UserId}", userId);

            var user = await _userProfileRepository.GetByUserIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Пользователь не найден при админ-обновлении UserId={UserId}", userId);
                return Result.Failure("Пользователь не найден.");
            }

            user.FirstName = dto.FirstName ?? user.FirstName;
            user.LastName = dto.LastName ?? user.LastName;
            user.MiddleName = dto.MiddleName ?? user.MiddleName;
            user.PhoneNumber = dto.PhoneNumber;
            user.BirthDate = dto.BirthDate;
            user.Organization = dto.Organization;
            user.Specialization = dto.Specialization;
            user.ParticipantType = dto.ParticipantType;
            user.Status = dto.Status;
            user.ApprovalStatus = dto.ApprovalStatus;
            user.Degree = dto.Degree;
            user.Position = dto.Position;

            await _userProfileRepository.UpdateAsync(user);
            _logger.LogInformation("Админ-обновление профиля завершено UserId={UserId}", userId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при админ-обновлении профиля UserId={UserId}", userId);
            return Result.Failure($"Произошла ошибка при обновлении профиля: {ex.Message}");
        }
    }
}
