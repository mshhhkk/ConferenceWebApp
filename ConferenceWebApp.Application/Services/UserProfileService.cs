using ConferenceWebApp.Application;
using ConferenceWebApp.Application.DTOs.Admin;
using ConferenceWebApp.Application.DTOs.PersonalAccountDTOs;
using ConferenceWebApp.Application.Interfaces.Repositories;
using ConferenceWebApp.Application.Interfaces.Services;
using ConferenceWebApp.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace ConferenceWebApp.Infrastructure.Services.Realization;

public class UserProfileService : IUserProfileService
{
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly ILogger<UserProfileService> _logger;

    public UserProfileService(IUserProfileRepository userProfileRepository, ILogger<UserProfileService> logger)
    {
        _userProfileRepository = userProfileRepository;
        _logger = logger;
    }

    public async Task<Result<UserProfileDTO>> GetByUserIdAsync(Guid userId)
    {
        _logger.LogInformation("Запрос профиля пользователя UserId={UserId}", userId);

        var userProfile = await _userProfileRepository.GetByUserIdAsync(userId);
        if (userProfile == null)
        {
            _logger.LogWarning("Профиль не найден UserId={UserId}", userId);
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

        _logger.LogInformation("Профиль пользователя UserId={UserId} успешно получен", userId);
        return Result<UserProfileDTO>.Success(dto);
    }

    public async Task<Result<AdminEditUserDTO>> AdminGetByUserIdAsync(Guid userId)
    {
        _logger.LogInformation("Админ-запрос профиля пользователя UserId={UserId}", userId);

        var userProfile = await _userProfileRepository.GetByUserIdAsync(userId);
        if (userProfile == null)
        {
            _logger.LogWarning("Профиль для админа не найден UserId={UserId}", userId);
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

        _logger.LogInformation("Админ-профиль UserId={UserId} успешно получен", userId);
        return Result<AdminEditUserDTO>.Success(dto);
    }

    public async Task<string> GetUserNameByEmailAsync(string email)
    {
        _logger.LogInformation("Получение сокращённого имени по Email={Email}", email);

        var user = await _userProfileRepository.GetUserProfileByEmail(email);
        if (user == null)
        {
            _logger.LogWarning("Профиль по Email не найден Email={Email}", email);
            // Поведение исходного кода приводило бы к NRE.
            // Явно логируем и кидаем исключение, чтобы не менять контракт скрытно.
            throw new Exception("Пользователь не найден");
        }

        var username = user.LastName + " " + user.FirstName[0] + ".";
        _logger.LogInformation("Сокращённое имя для Email={Email}: {Username}", email, username);
        return username;
    }

    public async Task<Result<List<AdminUserProfileDTO>>> GetAllAsync()
    {
        _logger.LogInformation("Запрос списка всех пользовательских профилей (админ)");

        var users = _userProfileRepository.GetAllList();
        if (users == null || !users.Any())
        {
            _logger.LogWarning("Список пользователей пуст");
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

        _logger.LogInformation("Возвращено {Count} пользовательских профилей", result.Count);
        return Result<List<AdminUserProfileDTO>>.Success(result);
    }
}
