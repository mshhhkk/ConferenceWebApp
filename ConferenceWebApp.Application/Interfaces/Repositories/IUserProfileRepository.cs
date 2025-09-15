using ConferenceWebApp.Domain.Entities;
using ConferenceWebApp.Domain.Enums;

namespace ConferenceWebApp.Application.Interfaces.Repositories;

public interface IUserProfileRepository
{
    Task<UserProfile?> GetByUserIdAsync(Guid userId);

    Task UpdateAsync(UserProfile userProfile);

    Task<ParticipantStatus> IsUserRegisteredForConferenceAsync(Guid userId);

    Task<UserProfile?> GetUserProfileByEmail(string email);

    Task<List<UserProfile>> GetUsersWithReceiptsAsync();

    Task DeleteAsync(UserProfile userProfile);

    Task<UserProfile?> FindByLastNameAsync(string lastName);

    Task<List<UserProfile>> SearchByFullNameAsync(string fullName);
    Task<UserProfile?> GetUserProfileByReportIdAsync(Guid reportId);

    IQueryable<UserProfile> GetAllQueryable();




}