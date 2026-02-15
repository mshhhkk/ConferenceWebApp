using ConferenceWebApp.Application;
using ConferenceWebApp.Application.DTOs;
using ConferenceWebApp.Application.DTOs.Admin;
using ConferenceWebApp.Application.DTOs.PersonalAccountDTOs;
using ConferenceWebApp.Application.Interfaces.Repositories;
using ConferenceWebApp.Application.Interfaces.Services;
using ConferenceWebApp.Domain.Entities;
using ConferenceWebApp.Domain.Enums;
using Microsoft.AspNetCore.Identity;
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
                BirthDate = new DateOnly(1970, 1, 1),
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

            userProfile.FirstName = dto.FirstName!;
            userProfile.LastName = dto.LastName!;
            userProfile.MiddleName = dto.MiddleName;
            userProfile.PhoneNumber = dto.PhoneNumber;
            userProfile.BirthDate = new DateOnly(1970, 1, 1);
            userProfile.Organization = dto.Organization;
            userProfile.Specialization = dto.Specialization;
            userProfile.Degree = dto.Degree!.Value;

            var wasStudent = userProfile.Position == Position.Student;
            var newPosition = dto.Position.HasValue ? dto.Position.Value : userProfile.Position;

            // 2) Обновляем позицию
            userProfile.Position = newPosition;

            // 3) Правила статуса
            if (newPosition == Position.Student)
            {
                // студентам сразу даём минимум ParticipationConfirmed
                if (userProfile.Status < ParticipantStatus.ParticipationConfirmed)
                    userProfile.Status = ParticipantStatus.ParticipationConfirmed;
            }
            else
            {
           
                // ушли со студента → откат c ParticipationConfirmed к ProfileCompleted
                if (wasStudent && userProfile.Status == ParticipantStatus.ParticipationConfirmed)
                    userProfile.Status = ParticipantStatus.ProfileCompleted;

                // и в любом случае минимум ProfileCompleted
                if (userProfile.Status < ParticipantStatus.ProfileCompleted)
                    userProfile.Status = ParticipantStatus.ProfileCompleted;
            }
            if (dto.RemovePhoto)
            {
                _logger.LogInformation("Удаление пользовательского фото UserId={UserId}", userId);
                HandlePhotoRemoval(userProfile);
            }
            else if (dto.Photo != null)
            {
                _logger.LogInformation("Обновление фото профиля UserId={UserId}, ContentType={ContentType}, Size={Size}",
                    userId, dto.Photo.ContentType, dto.Photo.Length);


                userProfile.PhotoUrl = await _fileService.UpdateFileAsync(
                    dto.Photo!,
                    userProfile.PhotoUrl!,
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

    private void HandlePhotoRemoval(UserProfile userProfile)
    {
        try
        {
            var path = userProfile.PhotoUrl;

            if (!string.IsNullOrWhiteSpace(path) &&
                !path.Equals(DefaultPhotoPath, StringComparison.OrdinalIgnoreCase))
            {
                _fileService.DeleteFile(path);
                _logger.LogInformation("Удален файл фото {Path} для UserId={UserId}", path, userProfile.UserId);
            }

            userProfile.PhotoUrl = DefaultPhotoPath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при удалении фото UserId={UserId}, Path={Path}",
                userProfile.UserId, userProfile.PhotoUrl);
            throw;
        }
    }


    public async Task<Result<InvitationDTO>> GenerateInvitationAsync(Guid userId)
    {
        try
        {
            _logger.LogInformation("Генерация приглашения UserId={UserId}", userId);

            var profile = await _userProfileRepository.GetByUserIdAsync(userId);
            if (profile is null)
                return Result<InvitationDTO>.Failure("Профиль пользователя не найден");

            var fio = string.Join(" ",
                new[] { profile.LastName, profile.FirstName, profile.MiddleName }
                .Where(s => !string.IsNullOrWhiteSpace(s)));

            static string Initial(string? s) =>
                string.IsNullOrWhiteSpace(s) ? "" : $"{char.ToUpper(s[0])}.";
            var greeting = $"Уважаемый(ая) {Initial(profile.FirstName)} {Initial(profile.MiddleName)} {profile.LastName}!";


            var reports = await _reportsRepository.GetApprovedReportsByUserIdAsync(userId);

            var list = new List<InvitationReportDTO>();
            foreach (var r in reports)
            {

                string workTypeText = r.WorkType switch
                {
                    WorkType.Стендовый => "стендового",
                    WorkType.Доклад => "устного доклада",
                    _ => "доклада"
                };

                var sectionText = EnumDescriptionGetter.Handle(r.Section);

                list.Add(new InvitationReportDTO
                {
                    Title = r.ReportTheme ?? "(без названия)",
                    WorkTypeText = workTypeText,
                    SectionText = sectionText
                });
            }

            var dto = new InvitationDTO
            {
                GreetingName = greeting,
                FullName = fio,
                Reports = list,
                IsStudent = profile.Position == Position.Student
            };

            _logger.LogInformation("Приглашение сформировано UserId={UserId}, Reports={Count}", userId, list.Count);
            return Result<InvitationDTO>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при генерации приглашения UserId={UserId}", userId);
            return Result<InvitationDTO>.Failure("Не удалось сформировать приглашение. Попробуйте позже.");
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
    public async Task<Result> DeleteAccountAsync(Guid userId)
    {
        try
        {
            _logger.LogInformation("Старт удаления аккаунта. UserId={UserId}", userId);

  
            var userReports = await _reportsRepository.GetReportsByUserIdAsync(userId);
            int deletedReports = 0;
            foreach (var r in userReports)
            {
                // при необходимости — удалить связанный файл доклада
                if (!string.IsNullOrWhiteSpace(r.FilePath))
                {
                    try { _fileService.DeleteFile(r.FilePath); }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Не удалось удалить файл доклада {Path} для ReportId={ReportId}", r.FilePath, r.Id);
                    }
                }

                await _reportsRepository.DeleteReportAsync(r.Id);
                deletedReports++;
            }
            _logger.LogInformation("Удалено докладов: {Count} для UserId={UserId}", deletedReports, userId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при удалении аккаунта. UserId={UserId}", userId);
            return Result.Failure("Произошла ошибка при удалении аккаунта.");
        }
    }
}

