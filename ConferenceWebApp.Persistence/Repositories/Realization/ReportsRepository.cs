using ConferenceWebApp.Application.Interfaces.Repositories;
using ConferenceWebApp.Domain.Entities;
using ConferenceWebApp.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ConferenceWebApp.Persistence.Repositories.Realization
{
    public class ReportsRepository : IReportsRepository
    {
        private readonly AppDbContext _context;

        public ReportsRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Reports>> GetReportsByUserIdAsync(Guid userId)
        {
            return await _context.Reports
                .Where(t => t.UserId == userId)
                .ToListAsync();
        }

        public async Task AddReportAsync(Reports report)
        {
            _context.Reports.Add(report);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateReportAsync(Reports report)
        {
            _context.Reports.Update(report);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Reports>> GetApprovedReportsByUserIdAsync(Guid userId)
        {
            return await _context.Reports
                .Where(t => t.UserId == userId && t.Status == ReportStatus.ExtendedThesisApproved)
                .ToListAsync();
        }

        public async Task<List<Reports>> GetReportsWithThesesByUserIdAsync(Guid userId)
        {
            return await _context.Reports
                .Where(t => t.UserId == userId && !string.IsNullOrEmpty(t.ExtThesis))
                .ToListAsync();
        }

        public async Task<Reports?> GetReportByIdAsync(Guid reportId)
        {
            return await _context.Reports
                .FirstOrDefaultAsync(t => t.Id == reportId);
        }

        public async Task<List<Reports>> GetPendingReportsAsync()
        {
            return await _context.Reports
                .Where(r => r.Status == ReportStatus.SubmittedThesis)
                .ToListAsync();
        }

        public async Task<List<Reports>> GetApprovedReportsAsync()
        {
            return await _context.Reports
                .Where(r => r.Status > ReportStatus.SubmittedThesis)
                .ToListAsync();
        }


        public async Task DeleteReportAsync(Guid id)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report != null)
            {
                _context.Reports.Remove(report);
                await _context.SaveChangesAsync();
            }
        }
    }
}