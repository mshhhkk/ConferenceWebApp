using ConferenceWebApp.Application.DTOs.PersonalAccountDTOs;
using ConferenceWebApp.Infrastructure.Services.Abstract;
using ConferenceWebApp.Application.Interfaces.Repositories;
namespace ConferenceWebApp.Infrastructure.Services.Realization;

public class UserService : IUserService
{
    private readonly IUserProfileRepository _userProfileRepository;
    public UserService(IUserProfileRepository userProfileRepository)
    {
        _userProfileRepository = userProfileRepository;
    }
    public async Task<UserProfileDTO> GetUserProfileAsync(Guid userId)
    {
        var userProfile = await _userProfileRepository.GetByUserIdAsync(userId);
        var dto = new UserProfileDTO
        {
            FullName = $"{userProfile?.FirstName ?? "Имя"} {userProfile?.LastName ?? "Фамилия"} {userProfile?.MiddleName ?? ""}",
            Email = userProfile?.User?.Email ?? string.Empty,
            PhoneNumber = userProfile?.PhoneNumber ?? "Не указан",
            BirthDate = (DateOnly)(userProfile?.BirthDate),
            Specialization = userProfile?.Specialization ?? "Не указана",
            Organization = userProfile?.Organization ?? "Не указана",
            PhotoUrl = userProfile?.PhotoUrl,
            ParticipantType = (Domain.Enums.ParticipantType)(userProfile?.ParticipantType),
            HasPaidFee = userProfile?.HasPaidFee ?? false,
            IsRegisteredForConference = userProfile?.IsRegisteredForConference ?? false,
            IsApprovedAnyReports = false
        };
        return dto;

    }
}
