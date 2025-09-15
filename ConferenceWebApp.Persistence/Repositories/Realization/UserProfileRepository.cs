using ConferenceWebApp.Domain.Entities;
using ConferenceWebApp.Application.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using ConferenceWebApp.Domain.Enums;

namespace ConferenceWebApp.Persistence.Repositories.Realization;

public class UserProfileRepository : IUserProfileRepository
{
    private readonly AppDbContext _context;

    public UserProfileRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<UserProfile?> GetByUserIdAsync(Guid userId)
    {
        var profile = await _context.UserProfile
            .FirstOrDefaultAsync(up => up.UserId == userId);
        return profile;
    }

    public async Task<UserProfile?> GetUserProfileByEmail(string email)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserName == email);

        if (user == null)
        {
            return null;
        }

        var userProfile = await _context.UserProfile
            .AsNoTracking()
            .FirstOrDefaultAsync(up => up.UserId == user.Id);

        return userProfile;
    }

    public async Task UpdateAsync(UserProfile userProfile)
    {
        _context.UserProfile.Update(userProfile);
        await _context.SaveChangesAsync();
    }

    public async Task<ParticipantStatus> IsUserRegisteredForConferenceAsync(Guid userId)
    {
        var userProfile = await _context.UserProfile
            .AsNoTracking()
            .FirstOrDefaultAsync(up => up.UserId == userId);

        return userProfile.Status;
    }

    public async Task<List<UserProfile>> GetUsersWithReceiptsAsync()
    {
        return await _context.UserProfile
            .Include(up => up.User) // Явная загрузка связанного User
            .Where(up => !string.IsNullOrEmpty(up.ReceiptFilePath))
            .ToListAsync();
    }

    public async Task DeleteAsync(UserProfile userProfile)
    {
        _context.UserProfile.Remove(userProfile);
        await _context.SaveChangesAsync();
    }

    public async Task<UserProfile?> FindByLastNameAsync(string lastName)
    {
        return await _context.UserProfile
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.LastName!.ToLower() == lastName.ToLower());
    }

    public async Task<List<UserProfile>> SearchByFullNameAsync(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return new List<UserProfile>();

        var terms = fullName
            .ToLower()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);

        return await _context.UserProfile
            .Where(up =>
                terms.All(term =>
                    up.FirstName.ToLower().Contains(term) ||
                    up.LastName.ToLower().Contains(term) ||
                    (up.MiddleName != null && up.MiddleName.ToLower().Contains(term))
                )
            )
            .ToListAsync();
    }
    public async Task<UserProfile?> GetUserProfileByReportIdAsync(Guid reportId)
    {
        return await _context.Reports
            .Where(r => r.Id == reportId)
            .Include(r => r.User)
            .ThenInclude(u => u.UserProfile)
            .Select(r => r.User.UserProfile)
            .FirstOrDefaultAsync();
    }
    public IQueryable<UserProfile> GetAllQueryable()
    {
        return _context.UserProfile.AsQueryable();
    }
}