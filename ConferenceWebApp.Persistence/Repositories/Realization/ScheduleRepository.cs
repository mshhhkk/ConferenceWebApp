using ConferenceWebApp.Application.Interfaces.Repositories;
using ConferenceWebApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConferenceWebApp.Persistence.Repositories.Realization
{
    public class ScheduleRepository : IScheduleRepository
    {
        private readonly AppDbContext _context;

        public ScheduleRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Schedule>> GetAllAsync()
        {
            return await _context.Schedule.ToListAsync();
        }

        public async Task<Schedule?> GetByIdAsync(Guid id)
        {
            return await _context.Schedule.FindAsync(id);
        }

        public async Task AddAsync(Schedule schedule)
        {
            _context.Schedule.Add(schedule);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Schedule schedule)
        {
            _context.Schedule.Update(schedule);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var schedule = await _context.Schedule.FindAsync(id);
            if (schedule != null)
            {
                _context.Schedule.Remove(schedule);
                await _context.SaveChangesAsync();
            }
        }
    }
}