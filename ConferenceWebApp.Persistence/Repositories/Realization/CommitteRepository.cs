using ConferenceWebApp.Domain.Entities;
using ConferenceWebApp.Application.Interfaces.Repositories;
using ConferenceWebApp.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ConferenceWebApp.Persistence.Repositories.Realization;

public class CommitteRepository : ICommitteRepository
{
    private readonly AppDbContext _context;

    public CommitteRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Committee?> GetByIdAsync(Guid id)
    {
        return await _context.Committee.FindAsync(id);
    }

    public async Task<IEnumerable<Committee>> GetAllAsync()
    {
        return await _context.Committee.ToListAsync();
    }

    public async Task AddAsync(Committee committee)
    {
        await _context.Committee.AddAsync(committee);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Committee committee)
    {
        _context.Committee.Update(committee);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var committee = await GetByIdAsync(id);
        if (committee != null)
        {
            _context.Committee.Remove(committee);
            await _context.SaveChangesAsync();
        }
    }
}