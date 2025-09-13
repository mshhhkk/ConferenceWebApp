using ConferenceWebApp.Domain.Entities;
using ConferenceWebApp.Application.DTOs;
using ConferenceWebApp.Application.DTOs.PersonalAccountDTOs;
using ConferenceWebApp.Application.DTOs.ReportsDTOs;
using ConferenceWebApp.Application.ViewModels;
using ConferenceWebApp.Application;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ConferenceWebApp.Application.Interfaces.Services;
using ConferenceWebApp.Application.Interfaces.Repositories;

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

    public async Task<Result<UserReportsViewModel>> GetUserReportsAsync(Guid userId)
    {
        var userProfile = await _userProfileRepository.GetByUserIdAsync(userId);
        if (userProfile == null || !userProfile.IsRegisteredForConference)
            return Result<UserReportsViewModel>.Failure("Пользователь не зарегистрирован на конференцию");

        var reports = await _reportRepository.GetReportsByUserIdAsync(userId);

        var userProfileDto = new UserProfileDTO
        {
            FullName = $"{userProfile.LastName} {userProfile.FirstName} {userProfile.MiddleName}".Trim(),
            Email = userProfile.User?.Email ?? string.Empty,
            PhoneNumber = userProfile.PhoneNumber ?? string.Empty,
            BirthDate = userProfile.BirthDate,
            Organization = userProfile.Organization ?? string.Empty,
            Specialization = userProfile.Specialization ?? string.Empty,
            PhotoUrl = userProfile.PhotoUrl,
            ParticipantType = userProfile.ParticipantType,
            HasPaidFee = userProfile.HasPaidFee,
            IsRegisteredForConference = userProfile.IsRegisteredForConference,
            IsApprovedAnyReports = reports.Any(r => r.IsApproved)
        };

        return Result<UserReportsViewModel>.Success(new UserReportsViewModel
        {
            UserProfile = userProfileDto,
            Reports = reports.Select(ToReportDTO).ToList()
        });
    }



    public async Task<Result<ReportDTO>> GetReportAsync(Guid reportId, Guid userId)
    {
        var result = await ValidateReportAccess(reportId, userId);
        if (!result.IsSuccess)
            return Result<ReportDTO>.Failure(result.ErrorMessage);

        return Result<ReportDTO>.Success(ToReportDTO(result.Value));
    }

    public async Task<Result<EditReportViewModel>> GetReportForEditAsync(Guid reportId, Guid userId)
    {
        var userProfile = await _userProfileRepository.GetByUserIdAsync(userId);
        if (userProfile == null || !userProfile.IsRegisteredForConference)
            return Result<EditReportViewModel>.Failure("Пользователь не зарегистрирован на конференцию");

        var reports = await _reportRepository.GetReportsByUserIdAsync(userId);

        var userProfileDto = new UserProfileDTO
        {
            FullName = $"{userProfile.LastName} {userProfile.FirstName} {userProfile.MiddleName}".Trim(),
            Email = userProfile.User?.Email ?? string.Empty,
            PhoneNumber = userProfile.PhoneNumber ?? string.Empty,
            BirthDate = userProfile.BirthDate,
            Organization = userProfile.Organization ?? string.Empty,
            Specialization = userProfile.Specialization ?? string.Empty,
            PhotoUrl = userProfile.PhotoUrl,
            ParticipantType = userProfile.ParticipantType,
            HasPaidFee = userProfile.HasPaidFee,
            IsRegisteredForConference = userProfile.IsRegisteredForConference,
            IsApprovedAnyReports = reports.Any(r => r.IsApproved)
        };
        var result = await ValidateReportAccess(reportId, userId);
        if (!result.IsSuccess)
            return Result<EditReportViewModel>.Failure(result.ErrorMessage);
        var report = await _reportRepository.GetReportByIdAsync(reportId);

        return Result<EditReportViewModel>.Success(new EditReportViewModel
        {
            UserProfile = userProfileDto,
            Report = ToEditReportDTO(report)
        });
    }

    public async Task<Result> CreateReportAsync(AddReportDTO dto, Guid userId)
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
                IsApproved = false,
                Section = dto.Section,
                WorkType = dto.WorkType,
                FilePath = filePath,
                IsAuthor = true,
                AuthorId = userId,
                UploadedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow
            };

            await _reportRepository.AddReportAsync(report);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failureure($"Ошибка при создании отчета: {ex.Message}");
        }
    }

    public async Task<Result> UpdateReportAsync(EditReportViewModel vm, Guid userId)
    {
        try
        {

            var result = await ValidateReportAccess(vm.Report.Id, userId);
            if (!result.IsSuccess)
                return Result.Failureure(result.ErrorMessage);

            var report = result.Value;
            report.ReportTheme = vm.Report.ReportTheme;
            report.Section = vm.Report.Section;
            report.WorkType = vm.Report.WorkType;
            report.LastUpdatedAt = DateTime.UtcNow;

            if (vm.Report.File != null)
            {
                var resultFile = ValidateFile(vm.Report.File);
                if (!resultFile.IsSuccess)
                    return result;

                report.FilePath = await _fileService.UpdateFileAsync(
                    vm.Report.File,
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
            return Result.Failureure($"Ошибка при обновлении отчета: {ex.Message}");
        }
    }

    public async Task<Result> DeleteReportAsync(Guid reportId, Guid userId)
    {
        try
        {
            var result = await ValidateReportAccess(reportId, userId);
            if (!result.IsSuccess)
                return Result.Failureure(result.ErrorMessage);

            _fileService.DeleteFile(result.Value.FilePath);
            await _reportRepository.DeleteReportAsync(reportId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failureure($"Ошибка при удалении отчета: {ex.Message}");
        }
    }

    public async Task<Result<FileStreamResult>> DownloadReportAsync(Guid reportId, Guid userId)
    {
        try
        {
            var result = await ValidateReportAccess(reportId, userId);
            if (!result.IsSuccess)
                return Result<FileStreamResult>.Failure(result.ErrorMessage);

            var (fileStream, contentType, fileName) = await _fileService.GetFileAsync(result.Value.FilePath);
            return Result<FileStreamResult>.Success(
                new FileStreamResult(fileStream, contentType) { FileDownloadName = fileName });
        }
        catch (Exception ex)
        {
            return Result<FileStreamResult>.Failure($"Ошибка при загрузке отчета: {ex.Message}");
        }
    }


    public async Task<Result<AddReportViewModel>> GetUserProfileForAddingReportsAsync(Guid userId)
    {
        var userProfile = await _userProfileRepository.GetByUserIdAsync(userId);
        var dto = new UserProfileDTO
        {
            FullName = $"{userProfile?.FirstName ?? "Имя"} {userProfile?.LastName ?? "Фамилия"} {userProfile?.MiddleName ?? ""}",
            Email = userProfile?.User?.Email ?? string.Empty,
            PhoneNumber = userProfile?.PhoneNumber ?? "Не указан",
            BirthDate = (DateOnly)(userProfile?.BirthDate),
            Specialization = userProfile?.Specialization ?? "Не указана",
            Organization = userProfile?.Organization ?? "Не указана",
            PhotoUrl = userProfile?.PhotoUrl,
            ParticipantType = (ConferenceWebApp.Domain.Enums.ParticipantType)(userProfile?.ParticipantType),
            HasPaidFee = userProfile?.HasPaidFee ?? false,
            IsRegisteredForConference = userProfile?.IsRegisteredForConference ?? false,
            IsApprovedAnyReports = false
        };
        var vm = new AddReportViewModel
        {
            UserProfile = dto,
            Report = new AddReportDTO()
        };
        return Result<AddReportViewModel>.Success(vm);
    }

    private async Task<Result<Reports>> ValidateReportAccess(Guid reportId, Guid userId)
    {
        var report = await _reportRepository.GetReportByIdAsync(reportId);
        if (report == null)
            return Result<Reports>.Failure("Отчет не найден");

        if (report.UserId != userId || report.AuthorId != userId)
            return Result<Reports>.Failure("Доступ к отчету запрещен");

        if (report.IsApproved)
            return Result<Reports>.Failure("Невозможно изменить утвержденный отчет");

        return Result<Reports>.Success(report);
    }

    private Result ValidateFile(IFormFile file)
    {
        var extension = Path.GetExtension(file.FileName).ToLower();
        if (!AllowedExtensions.Contains(extension))
            return Result.Failureure("Допустимы только файлы .doc и .docx");

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
        IsApproved = report.IsApproved
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