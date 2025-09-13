using ConferenceWebApp.Domain.Entities;
using ConferenceWebApp.Application.DTOs.Admin;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceWebApp.Infrastructure.Services.Abstract.Admin;

public interface IReportAdminService
{
    Task<AdminFilteredReportsListDTO> GetFilteredReportsAsync(string? search);

    //Task<FileStreamResult?> DownloadReportFileAsync(Guid id);

    Task<bool> ApproveReportAsync(Guid id);

    Task<bool> RollbackReportAsync(Guid id);

    Task<Reports?> GetReportByIdAsync(Guid id);
    Task<string?> GetReportFilePathAsync(Guid id);
}