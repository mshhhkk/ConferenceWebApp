using ConferenceWebApp.Domain.Entities;

namespace ConferenceWebApp.Application.Interfaces.Repositories;

public interface ITwoFactorCodeRepository
{
    Task AddAsync(TwoFactorCode twoFactorCode);

    Task<TwoFactorCode?> GetLatestByEmailAsync(string email);

    Task RemoveAsync(TwoFactorCode twoFactorCode);
}