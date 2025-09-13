using ConferenceWebApp.Domain.Entities;

namespace ConferenceWebApp.Application.Interfaces.Repositories
{
    public interface IScheduleRepository
    {
        Task<List<Schedule>> GetAllAsync();

        Task<Schedule?> GetByIdAsync(Guid id);

        Task AddAsync(Schedule schedule);

        Task UpdateAsync(Schedule schedule);

        Task DeleteAsync(Guid id);
    }
}