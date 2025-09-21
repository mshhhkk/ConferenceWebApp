using ConferenceWebApp.Application.DTOs.Admin;
using ConferenceWebApp.Domain.Entities;

namespace ConferenceWebApp.Application.Interfaces.Services.Admin;

public interface IReportAdminService
{
    Task<AdminFilteredReportsListDTO> GetFilteredReportsAsync(string? search);

    //Task<FileStreamResult?> DownloadReportFileAsync(Guid id);

    Task<bool> ApproveReportAsync(Guid id);

    Task<bool> RollbackReportAsync(Guid id);

    Task<Reports?> GetReportByIdAsync(Guid id);
    Task<string?> GetReportFilePathAsync(Guid id);
    Task RejectReportAsync(Guid id, string comment);

    Task<bool> RejectExtendedThesisAsync(Guid id, string comment);

    Task<bool> ApproveExtendedThesisAsync(Guid id);


}