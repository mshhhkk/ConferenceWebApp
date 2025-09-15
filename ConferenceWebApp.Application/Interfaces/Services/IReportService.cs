using ConferenceWebApp.Application.DTOs.ReportsDTOs;
using ConferenceWebApp.Application.ViewModels;
using ConferenceWebApp.Application;
using Microsoft.AspNetCore.Mvc;
using ConferenceWebApp.Application.DTOs;
using System.Threading.Tasks;

public interface IReportService
{
    Task<Result<List<ReportDTO>>> GetReportsByUserIdAsync(Guid userId);

    Task<Result<EditReportDTO>> GetReportForEditAsync(Guid reportId, Guid userId);

    Task<Result> AddReportAsync(AddReportDTO dto, Guid userId);

    Task<Result> UpdateReportAsync(EditReportDTO dto, Guid userId);

    Task<Result> DeleteReportAsync(Guid reportId, Guid userId);

    Task<Result<FileStreamResult>> DownloadReportAsync(Guid reportId, Guid userId);

}