using ConferenceWebApp.Application.DTOs.ReportsDTOs;
using ConferenceWebApp.Application.ViewModels;
using ConferenceWebApp.Application;
using Microsoft.AspNetCore.Mvc;

public interface IReportService
{
    Task<Result<UserReportsViewModel>> GetUserReportsAsync(Guid userId);

    Task<Result<EditReportViewModel>> GetReportForEditAsync(Guid reportId, Guid userId);

    Task<Result<ReportDTO>> GetReportAsync(Guid reportId, Guid userId);

    Task<Result> CreateReportAsync(AddReportDTO dto, Guid userId);

    Task<Result> UpdateReportAsync(EditReportViewModel vm, Guid userId);

    Task<Result> DeleteReportAsync(Guid reportId, Guid userId);

    Task<Result<FileStreamResult>> DownloadReportAsync(Guid reportId, Guid userId);

    Task<Result<AddReportViewModel>> GetUserProfileForAddingReportsAsync(Guid userId);
}