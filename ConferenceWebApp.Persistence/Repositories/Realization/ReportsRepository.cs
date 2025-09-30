using ConferenceWebApp.Application.Interfaces.Repositories;
using ConferenceWebApp.Domain.Entities;
using ConferenceWebApp.Domain.Enums;
using ConferenceWebApp.Persistence;
using Microsoft.EntityFrameworkCore;

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

    // Добавить новый отчет
    public async Task AddReportAsync(Reports report)
    {
        _context.Reports.Add(report);
        await _context.SaveChangesAsync();
    }

    // Обновить отчет
    public async Task UpdateReportAsync(Reports report)
    {
        _context.Reports.Update(report);
        await _context.SaveChangesAsync();
    }

    // Получить одобренные отчеты по UserId
    public async Task<List<Reports>> GetApprovedReportsByUserIdAsync(Guid userId)
    {
        return await _context.Reports
            .Where(t => t.UserId == userId && t.Status == ReportStatus.ThesisApproved)
            .ToListAsync();
    }

    // Получить отчеты с расширенными тезисами по UserId
    public async Task<List<Reports>> GetReportsWithThesesByUserIdAsync(Guid userId)
    {
        return await _context.Reports
            .Where(t => t.UserId == userId && !string.IsNullOrEmpty(t.ExtThesis))
            .ToListAsync();
    }

    // Получить отчет по его ID
    public async Task<Reports?> GetReportByIdAsync(Guid reportId)
    {
        return await _context.Reports
            .FirstOrDefaultAsync(t => t.Id == reportId);
    }

    // Получить ожидающие одобрения отчеты
    public async Task<List<Reports>> GetPendingReportsAsync()
    {
        return await _context.Reports
            .Where(r => r.Status == ReportStatus.SubmittedThesis)
            .ToListAsync();
    }

    // Получить одобренные отчеты
    public async Task<List<Reports>> GetApprovedReportsAsync()
    {
        return await _context.Reports
            .Where(r => r.Status > ReportStatus.SubmittedThesis && r.Status != ReportStatus.ThesisReturnedForCorrection)
            .ToListAsync();
    }

    // Получить отклоненные отчеты
    public async Task<List<Reports>> GetRejectedReportsAsync()
    {
        return await _context.Reports
            .Where(r => r.Status == ReportStatus.ThesisReturnedForCorrection || r.Status == ReportStatus.ExtendedThesisReturnedForCorrection)
            .ToListAsync();
    }

    // Получить предложенные расширенные тезисы
    public async Task<List<Reports>> GetPendingExtendedThesesAsync()
    {
        return await _context.Reports
            .Where(r => r.Status == ReportStatus.SubmittedExtendedThesis)
            .ToListAsync();
    }

    // Получить одобренные расширенные тезисы
    public async Task<List<Reports>> GetApprovedExtendedThesesAsync()
    {
        return await _context.Reports
            .Where(r => r.Status == ReportStatus.ExtendedThesisApproved)
            .ToListAsync();
    }

    // Получить отклоненные расширенные тезисы
    public async Task<List<Reports>> GetRejectedExtendedThesesAsync()
    {
        return await _context.Reports
            .Where(r => r.Status == ReportStatus.ExtendedThesisReturnedForCorrection)
            .ToListAsync();
    }

    // Удалить отчет
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
