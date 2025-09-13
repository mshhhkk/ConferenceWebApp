using ConferenceWebApp.Domain.Entities;

namespace ConferenceWebApp.Application.Interfaces.Repositories
{
    public interface ICommitteRepository
    {
        Task<Committee?> GetByIdAsync(Guid id);

        Task<IEnumerable<Committee>> GetAllAsync();

        Task AddAsync(Committee committee);

        Task UpdateAsync(Committee committee);

        Task DeleteAsync(Guid id);
    }
}