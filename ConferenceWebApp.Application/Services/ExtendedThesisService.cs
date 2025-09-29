using ConferenceWebApp.Application;
using ConferenceWebApp.Application.DTOs.ReportsDTOs;
using ConferenceWebApp.Application.Interfaces.Repositories;
using ConferenceWebApp.Application.Interfaces.Services;
using ConferenceWebApp.Domain.Entities;
using ConferenceWebApp.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace ConferenceWebApp.Infrastructure.Services.Realization;

public class ExtendedThesisService : IExtendedThesisService
{
    private readonly IReportsRepository _reportsRepository;
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly ILogger<ExtendedThesisService> _logger;

    public ExtendedThesisService(
        IReportsRepository reportsRepository,
        IUserProfileRepository userProfileRepository,
        ILogger<ExtendedThesisService> logger) 
    {
        _reportsRepository = reportsRepository;
        _userProfileRepository = userProfileRepository;
        _logger = logger;
    }

    public async Task<Result<ExtendedThesisDTO>> GetExtendedThesisesAsync(Guid userId)
    {
        _logger.LogInformation("Загрузка расширенных тезисов для пользователя {UserId}", userId);

        var profile = await _userProfileRepository.GetByUserIdAsync(userId);
        if (profile == null || profile.Status < ParticipantStatus.ProfileCompleted)
        {
            _logger.LogWarning("Пользователь {UserId} не зарегистрирован или не завершил профиль", userId);
            return Result<ExtendedThesisDTO>.Failure("Вы не зарегистрированы на конференцию.");
        }

        var reports = await _reportsRepository.GetApprovedReportsByUserIdAsync(userId);
        if (reports == null || reports.Count == 0)
        {
            _logger.LogWarning("У пользователя {UserId} нет одобренных докладов", userId);
            return Result<ExtendedThesisDTO>.Failure("У вас нет одобренных докладов.");
        }

        var dto = new ExtendedThesisDTO
        {
            ReportsWithTheses = reports
                .Where(r => r.ExtThesis != null && r.Status == ReportStatus.ThesisApproved)
                .Select(r => new Reports
                {
                    Id = r.Id,
                    ReportTheme = r.ReportTheme,
                    Section = r.Section,
                    WorkType = r.WorkType,
                    UploadedAt = r.UploadedAt,
                    LastUpdatedAt = r.LastUpdatedAt,
                }).ToList(),
            ReportsWithoutTheses = reports
                .Where(r => r.ExtThesis == null && r.Status == ReportStatus.ThesisApproved)
                .Select(r => new Reports
                {
                    Id = r.Id,
                    ReportTheme = r.ReportTheme,
                    Section = r.Section,
                    WorkType = r.WorkType,
                    UploadedAt = r.UploadedAt,
                    LastUpdatedAt = r.LastUpdatedAt,
                }).ToList()
        };

        _logger.LogInformation("Для пользователя {UserId} найдено {With} докладов с тезисами и {Without} без",
            userId, dto.ReportsWithTheses.Count, dto.ReportsWithoutTheses.Count);

        return Result<ExtendedThesisDTO>.Success(dto);
    }

    public async Task<Result<EditExtendedThesisDTO>> GetThesisAsync(Guid userId, Guid reportId)
    {
        _logger.LogInformation("Запрос расширенного тезиса ReportId={ReportId}, UserId={UserId}", reportId, userId);

        var profile = await _userProfileRepository.GetByUserIdAsync(userId);
        if (profile == null || profile.Status < ParticipantStatus.ProfileCompleted)
        {
            _logger.LogWarning("Пользователь {UserId} не зарегистрирован или не завершил профиль", userId);
            return Result<EditExtendedThesisDTO>.Failure("Вы не зарегистрированы на конференцию.");
        }

        var report = await _reportsRepository.GetReportByIdAsync(reportId);

        if (report == null)
        {
            _logger.LogWarning("Доклад ReportId={ReportId} не найден для UserId={UserId}", reportId, userId);
            return Result<EditExtendedThesisDTO>.Failure("Доклад не найден");
        }

        if (report.Status < ReportStatus.ThesisApproved)
        {
            _logger.LogWarning("Доклад ReportId={ReportId} ещё не одобрен для расширенного тезиса", reportId);
            return Result<EditExtendedThesisDTO>.Failure("У вас нет доступных одобренных тезисов");
        }

        var dto = new EditExtendedThesisDTO
        {
            ReportId = report.Id,
            ReportTheme = report.ReportTheme,
            Organization = profile.Organization!,
            ExtThesis = report.ExtThesis
        };

        _logger.LogInformation("Доклад ReportId={ReportId} загружен для редактирования", report.Id);

        return Result<EditExtendedThesisDTO>.Success(dto);
    }

    public async Task<Result> UpdateExtendedThesisAsync(Guid userId, EditExtendedThesisDTO dto)
    {
        _logger.LogInformation("Обновление расширенного тезиса ReportId={ReportId}, UserId={UserId}", dto.ReportId, userId);

        var report = await _reportsRepository.GetReportByIdAsync(dto.ReportId);
        if (report == null)
        {
            _logger.LogWarning("Доклад ReportId={ReportId} не найден при обновлении", dto.ReportId);
            return Result.Failure("Доклад не найден");
        }

        if (report.Status < ReportStatus.ThesisApproved)
        {
            _logger.LogWarning("Доклад ReportId={ReportId} не одобрен, обновление невозможно", dto.ReportId);
            return Result.Failure("У вас нет доступных одобренных тезисов");
        }

        report.ExtThesis = dto.ExtThesis;
        report.Status = ReportStatus.SubmittedExtendedThesis;
        await _reportsRepository.UpdateReportAsync(report);

        _logger.LogInformation("Доклад ReportId={ReportId} успешно обновлён пользователем {UserId}", dto.ReportId, userId);

        return Result.Success();
    }
}
