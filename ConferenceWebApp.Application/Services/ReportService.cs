using ConferenceWebApp.Application;
using ConferenceWebApp.Application.DTOs;
using ConferenceWebApp.Application.DTOs.ReportsDTOs;
using ConferenceWebApp.Application.Interfaces.Repositories;
using ConferenceWebApp.Application.Interfaces.Services;
using ConferenceWebApp.Domain.Entities;
using ConferenceWebApp.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

public class ReportService : IReportService
{
    private readonly IReportsRepository _reportRepository;
    private readonly IFileService _fileService;
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly ILogger<ReportService> _logger;

    private const long MaxFileSize = 50 * 1024 * 1024;
    private static readonly string[] AllowedExtensions = { ".doc", ".docx" };

    public ReportService(
        IReportsRepository reportRepository,
        IFileService fileService,
        IUserProfileRepository userProfileRepository,
        ILogger<ReportService> logger)
    {
        _reportRepository = reportRepository;
        _fileService = fileService;
        _userProfileRepository = userProfileRepository;
        _logger = logger;
    }

    public async Task<Result<List<ReportDTO>>> GetReportsByUserIdAsync(Guid userId)
    {
        try
        {
            _logger.LogInformation("Запрошен список докладов для пользователя {UserId}", userId);

            var userProfile = await _userProfileRepository.GetByUserIdAsync(userId);
            if (userProfile == null || userProfile.Status == 0)
            {
                _logger.LogWarning("Профиль не найден или не зарегистрирован, UserId={UserId}", userId);
                return Result<List<ReportDTO>>.Failure("Пользователь не зарегистрирован на конференцию, заполните личную информацию");
            }

            var reports = await _reportRepository.GetReportsByUserIdAsync(userId);
            var list = reports.Select(ToReportDTO).ToList();

            _logger.LogInformation("Найдено {Count} доклад(ов) для UserId={UserId}", list.Count, userId);
            return Result<List<ReportDTO>>.Success(list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении списка докладов, UserId={UserId}", userId);
            return Result<List<ReportDTO>>.Failure($"Ошибка при загрузке списка докладов: {ex.Message}");
        }
    }

    public async Task<Result<EditReportDTO>> GetReportForEditAsync(Guid reportId, Guid userId)
    {
        try
        {
            _logger.LogInformation("Получение доклада для редактирования ReportId={ReportId}, UserId={UserId}", reportId, userId);

            var report = await _reportRepository.GetReportByIdAsync(reportId);
            if (report == null)
            {
                _logger.LogWarning("Доклад не найден ReportId={ReportId}", reportId);
                return Result<EditReportDTO>.Failure("Доклад не найден");
            }

            if (report.UserId != userId || report.AuthorId != userId)
            {
                _logger.LogWarning("Попытка доступа к чужому докладу ReportId={ReportId}, UserId={UserId}", reportId, userId);
                return Result<EditReportDTO>.Failure("Доступ к отчету запрещен");
            }

            if (report.Status == ReportStatus.ThesisApproved)
            {
                _logger.LogWarning("Попытка изменить утвержденный доклад ReportId={ReportId}", reportId);
                return Result<EditReportDTO>.Failure("Невозможно изменить утвержденный отчет");
            }

            return Result<EditReportDTO>.Success(ToEditReportDTO(report));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении доклада ReportId={ReportId}, UserId={UserId}", reportId, userId);
            return Result<EditReportDTO>.Failure($"Ошибка при получении доклада: {ex.Message}");
        }
    }

    public async Task<Result<List<EditReportDTO>>> GetReportsForEditAsync(Guid userId)
    {
        try
        {
            _logger.LogInformation("Получение списка докладов для редактирования UserId={UserId}", userId);

            var reports = await _reportRepository.GetReportsByUserIdAsync(userId);
            var editReportDTOs = reports.Select(report => ToEditReportDTO(report)).ToList();

            _logger.LogInformation("Готово: {Count} доклад(ов) для редактирования, UserId={UserId}", editReportDTOs.Count, userId);
            return Result<List<EditReportDTO>>.Success(editReportDTOs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении докладов для редактирования UserId={UserId}", userId);
            return Result<List<EditReportDTO>>.Failure($"Ошибка при загрузке докладов: {ex.Message}");
        }
    }

    public async Task<Result> AddReportAsync(AddReportDTO dto, Guid userId)
    {
        try
        {
            _logger.LogInformation("Добавление доклада UserId={UserId}, Theme={Theme}", userId, dto?.ReportTheme);

            var result = ValidateFile(dto.File);
            if (!result.IsSuccess)
            {
                _logger.LogWarning("Валидация файла не пройдена: {Reason}", result.ErrorMessage);
                return result;
            }

            var filePath = await _fileService.SaveFileAsync(
                dto.File,
                "uploads",
                new[] { "application/msword", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
                MaxFileSize);

            var report = new Reports
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ReportTheme = dto.ReportTheme,
                Section = dto.Section,
                WorkType = dto.WorkType,
                FilePath = filePath,
                IsAuthor = true,
                AuthorId = userId,
                UploadedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow,
                Status = ReportStatus.SubmittedThesis,
                RejectionComment = string.Empty,
                TransferStatus = ReportTransferStatus.None
            };

            await _reportRepository.AddReportAsync(report);
            _logger.LogInformation("Доклад создан ReportId={ReportId}, UserId={UserId}", report.Id, userId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании отчета UserId={UserId}", userId);
            return Result.Failure($"Ошибка при создании отчета: {ex.Message}");
        }
    }

    public async Task<Result> UpdateReportAsync(EditReportDTO dto, Guid userId)
    {
        try
        {
            _logger.LogInformation("Обновление доклада ReportId={ReportId}, UserId={UserId}", dto.Id, userId);

            var report = await _reportRepository.GetReportByIdAsync(dto.Id);
            report.ReportTheme = dto.ReportTheme;
            report.Section = dto.Section;
            report.WorkType = dto.WorkType;
            report.LastUpdatedAt = DateTime.UtcNow;

            if (dto.File != null)
            {
                var resultFile = ValidateFile(dto.File);
                if (!resultFile.IsSuccess)
                {
                    _logger.LogWarning("Валидация файла при обновлении не пройдена: {Reason}", resultFile.ErrorMessage);
                    return resultFile;
                }

                report.FilePath = await _fileService.UpdateFileAsync(
                    dto.File,
                    report.FilePath,
                    "uploads",
                    new[] { "application/msword", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
                    MaxFileSize);
            }

            await _reportRepository.UpdateReportAsync(report);
            _logger.LogInformation("Доклад обновлён ReportId={ReportId}, UserId={UserId}", dto.Id, userId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении отчета ReportId={ReportId}, UserId={UserId}", dto.Id, userId);
            return Result.Failure($"Ошибка при обновлении отчета: {ex.Message}");
        }
    }

    public async Task<Result> DeleteReportAsync(Guid reportId, Guid userId)
    {
        try
        {
            _logger.LogInformation("Удаление доклада ReportId={ReportId}, UserId={UserId}", reportId, userId);

            var report = await _reportRepository.GetReportByIdAsync(reportId);
            if (report == null)
            {
                _logger.LogWarning("Доклад для удаления не найден ReportId={ReportId}", reportId);
                return Result<EditReportDTO>.Failure("Доклад не найден");
            }

            if (report.UserId != userId || report.AuthorId != userId)
            {
                _logger.LogWarning("Попытка удалить чужой доклад ReportId={ReportId}, UserId={UserId}", reportId, userId);
                return Result<EditReportDTO>.Failure("Доступ к отчету запрещен");
            }

            if (report.Status == ReportStatus.ThesisApproved)
            {
                _logger.LogWarning("Попытка удалить утвержденный доклад ReportId={ReportId}", reportId);
                return Result<EditReportDTO>.Failure("Невозможно изменить утвержденный отчет");
            }

            _fileService.DeleteFile(report.FilePath);
            await _reportRepository.DeleteReportAsync(reportId);

            _logger.LogInformation("Доклад удалён ReportId={ReportId}, UserId={UserId}", reportId, userId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при удалении отчета ReportId={ReportId}, UserId={UserId}", reportId, userId);
            return Result.Failure($"Ошибка при удалении отчета: {ex.Message}");
        }
    }

    public async Task<Result<FileStreamResult>> DownloadReportAsync(Guid reportId, Guid userId)
    {
        try
        {
            _logger.LogInformation("Скачивание доклада ReportId={ReportId}, UserId={UserId}", reportId, userId);

            var report = await _reportRepository.GetReportByIdAsync(reportId);
            if (report == null)
            {
                _logger.LogWarning("Доклад для скачивания не найден ReportId={ReportId}", reportId);
                return Result<FileStreamResult>.Failure("Доклад не найден");
            }

            if (report.UserId != userId || report.AuthorId != userId)
            {
                _logger.LogWarning("Попытка скачать чужой доклад ReportId={ReportId}, UserId={UserId}", reportId, userId);
                return Result<FileStreamResult>.Failure("Доступ к отчету запрещен");
            }

            var (fileStream, contentType, fileName) = await _fileService.GetFileAsync(report.FilePath);

            _logger.LogInformation("Доклад отдан на скачивание ReportId={ReportId}, UserId={UserId}", reportId, userId);
            return Result<FileStreamResult>.Success(
                new FileStreamResult(fileStream, contentType) { FileDownloadName = fileName });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при скачивании отчета ReportId={ReportId}, UserId={UserId}", reportId, userId);
            return Result<FileStreamResult>.Failure($"Ошибка при загрузке отчета: {ex.Message}");
        }
    }

    private Result ValidateFile(IFormFile file)
    {
        var extension = Path.GetExtension(file.FileName).ToLower();
        if (!AllowedExtensions.Contains(extension))
            return Result.Failure("Допустимы только файлы .doc и .docx");

        return Result.Success();
    }

    private ReportDTO ToReportDTO(Reports report) => new()
    {
        Id = report.Id,
        ReportTheme = report.ReportTheme,
        Section = report.Section.ToString(),
        WorkType = report.WorkType.ToString(),
        UploadedAt = TimeZoneInfo.ConvertTimeFromUtc(report.UploadedAt, TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time")),
        LastUpdatedAt = TimeZoneInfo.ConvertTimeFromUtc(report.LastUpdatedAt, TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time")),
    };

    private EditReportDTO ToEditReportDTO(Reports report) => new()
    {
        Id = report.Id,
        ReportTheme = report.ReportTheme,
        Section = report.Section,
        WorkType = report.WorkType,
        CurrentFileName = Path.GetFileName(report.FilePath),
        CurrentFilePath = report.FilePath,
        CurrentFileUploadDate = report.LastUpdatedAt
    };
}
