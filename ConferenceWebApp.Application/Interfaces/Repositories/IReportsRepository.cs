using ConferenceWebApp.Domain.Entities;

namespace ConferenceWebApp.Application.Interfaces.Repositories
{
    public interface IReportsRepository
    {
        Task<List<Reports>> GetReportsByUserIdAsync(Guid userId);

        Task AddReportAsync(Reports report);

        Task UpdateReportAsync(Reports report);

        Task<List<Reports>> GetApprovedReportsByUserIdAsync(Guid userId);

        Task<List<Reports>> GetReportsWithThesesByUserIdAsync(Guid userId);

        Task<Reports?> GetReportByIdAsync(Guid reportId);

        Task<List<Reports>> GetPendingReportsAsync();

        Task<List<Reports>> GetApprovedReportsAsync();

        Task DeleteReportAsync(Guid id);
    }
}