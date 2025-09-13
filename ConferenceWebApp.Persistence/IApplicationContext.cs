using ConferenceWebApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConferenceWebApp.Persistence
{
    public interface IApplicationContext
    {
        public DbSet<User> Users { get; }

        public Task<int> SaveChangesAsync(CancellationToken ct = default);
    }
}