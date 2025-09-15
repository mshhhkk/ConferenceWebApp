using ConferenceWebApp.Application.Interfaces.Repositories;
using ConferenceWebApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConferenceWebApp.Persistence.Repositories.Realization
{
    public class TwoFactorCodeRepository : ITwoFactorCodeRepository
    {
        private readonly AppDbContext _dbContext;

        public TwoFactorCodeRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(TwoFactorCode twoFactorCode)
        {
            _dbContext.TwoFactorCode.Add(twoFactorCode);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<TwoFactorCode?> GetLatestByEmailAsync(string email)
        {
            return await _dbContext.TwoFactorCode
                .Include(tfc => tfc.User)
                .Where(tfc => tfc.User.Email == email)
                .OrderByDescending(tfc => tfc.ExpirationTime)
                .FirstOrDefaultAsync();
        }

        public async Task RemoveAsync(TwoFactorCode twoFactorCode)
        {
            _dbContext.TwoFactorCode.Remove(twoFactorCode);
            await _dbContext.SaveChangesAsync();
        }
    }
}