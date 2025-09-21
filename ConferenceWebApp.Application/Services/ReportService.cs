using ConferenceWebApp.Application;
using ConferenceWebApp.Application.DTOs;
using ConferenceWebApp.Application.DTOs.ReportsDTOs;
using ConferenceWebApp.Application.Interfaces.Repositories;
using ConferenceWebApp.Application.Interfaces.Services;
using ConferenceWebApp.Domain.Entities;
using ConferenceWebApp.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class ReportService : IReportService
{
    private readonly IReportsRepository _reportRepository;
    private readonly IFileService _fileService;
    private readonly IUserProfileRepository _userProfileRepository;
    private const long MaxFileSize = 50 * 1024 * 1024;
    private static readonly string[] AllowedExtensions = { ".doc", ".docx" };

    public ReportService(
        IReportsRepository reportRepository,
        IFileService fileService,
        IUserProfileRepository userProfileRepository)
    {
        _reportRepository = reportRepository;
        _fileService = fileService;
        _userProfileRepository = userProfileRepository;
    }

    public async Task<Result<List<ReportDTO>>> GetReportsByUserIdAsync(Guid userId)
    {
        var userProfile = await _userProfileRepository.GetByUserIdAsync(userId);
        if (userProfile == null || userProfile.Status == 0)
            return Result<List<ReportDTO>>.Failure("Пользователь не зарегистрирован на конференцию, заполните личную информацию");

        var reports = await _reportRepository.GetReportsByUserIdAsync(userId);
        return Result<List<ReportDTO>>.Success(reports.Select(ToReportDTO).ToList());
    }


    public async Task<Result<EditReportDTO>> GetReportForEditAsync(Guid reportId, Guid userId)
    {
        var report = await _reportRepository.GetReportByIdAsync(reportId);
        if (report == null)
            return Result<EditReportDTO>.Failure("Доклад не найден");

        if (report.UserId != userId || report.AuthorId != userId)
            return Result<EditReportDTO>.Failure("Доступ к отчету запрещен");

        if (report.Status == ReportStatus.ThesisApproved)
            return Result<EditReportDTO>.Failure("Невозможно изменить утвержденный отчет");

        return Result<EditReportDTO>.Success(ToEditReportDTO(report));

    }

    public async Task<Result<List<EditReportDTO>>> GetReportsForEditAsync(Guid userId)
    {
        var reports = await _reportRepository.GetReportsByUserIdAsync(userId);
        var editReportDTOs = reports.Select(report => ToEditReportDTO(report)).ToList();

        return Result<List<EditReportDTO>>.Success(editReportDTOs);

    }
    public async Task<Result> AddReportAsync(AddReportDTO dto, Guid userId)
    {
        try
        {
            var result = ValidateFile(dto.File);
            if (!result.IsSuccess)
                return result;

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
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Ошибка при создании отчета: {ex.Message}");
        }
    }

    public async Task<Result> UpdateReportAsync(EditReportDTO dto, Guid userId)
    {
        try
        {
            var report = await _reportRepository.GetReportByIdAsync(dto.Id);
            report.ReportTheme = dto.ReportTheme;
            report.Section = dto.Section;
            report.WorkType = dto.WorkType;
            report.LastUpdatedAt = DateTime.UtcNow;

            if (dto.File != null)
            {
                var resultFile = ValidateFile(dto.File);
                if (!resultFile.IsSuccess)
                    return resultFile;

                report.FilePath = await _fileService.UpdateFileAsync(
                    dto.File,
                    report.FilePath,
                    "uploads",
                    new[] { "application/msword", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
                    MaxFileSize);
            }

            await _reportRepository.UpdateReportAsync(report);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Ошибка при обновлении отчета: {ex.Message}");
        }
    }
    public async Task<Result> DeleteReportAsync(Guid reportId, Guid userId)
    {
        try
        {
            var report = await _reportRepository.GetReportByIdAsync(reportId);
            if (report == null)
                return Result<EditReportDTO>.Failure("Доклад не найден");

            if (report.UserId != userId || report.AuthorId != userId)
                return Result<EditReportDTO>.Failure("Доступ к отчету запрещен");

            if (report.Status == ReportStatus.ThesisApproved)
                return Result<EditReportDTO>.Failure("Невозможно изменить утвержденный отчет");

            _fileService.DeleteFile(report.FilePath);
            await _reportRepository.DeleteReportAsync(reportId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Ошибка при удалении отчета: {ex.Message}");
        }
    }

    public async Task<Result<FileStreamResult>> DownloadReportAsync(Guid reportId, Guid userId)
    {
        try
        {
            var report = await _reportRepository.GetReportByIdAsync(reportId);
            if (report == null)
                return Result<FileStreamResult>.Failure("Доклад не найден");

            if (report.UserId != userId || report.AuthorId != userId)
                return Result<FileStreamResult>.Failure("Доступ к отчету запрещен");

            var (fileStream, contentType, fileName) = await _fileService.GetFileAsync(report.FilePath);
            return Result<FileStreamResult>.Success(
                new FileStreamResult(fileStream, contentType) { FileDownloadName = fileName });
        }
        catch (Exception ex)
        {
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