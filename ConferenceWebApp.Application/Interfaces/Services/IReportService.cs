using ConferenceWebApp.Application;
using ConferenceWebApp.Application.DTOs;
using ConferenceWebApp.Application.DTOs.ReportsDTOs;
using Microsoft.AspNetCore.Mvc;

public interface IReportService
{
    Task<Result<List<ReportDTO>>> GetReportsByUserIdAsync(Guid userId);

    Task<Result<EditReportDTO>> GetReportForEditAsync(Guid reportId, Guid userId);

    Task<Result<List<EditReportDTO>>> GetReportsForEditAsync(Guid userId);
    Task<Result> AddReportAsync(AddReportDTO dto, Guid userId);

    Task<Result> UpdateReportAsync(EditReportDTO dto, Guid userId);
    Task<Result> DeleteReportAsync(Guid reportId, Guid userId);

    Task<Result<FileStreamResult>> DownloadReportAsync(Guid reportId, Guid userId);

}