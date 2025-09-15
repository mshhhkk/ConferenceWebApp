using ConferenceWebApp.Application.DTOs.Admin;
using ConferenceWebApp.Infrastructure.Services.Abstract.Admin;
using ConferenceWebApp.Application;
using Microsoft.EntityFrameworkCore;
using ConferenceWebApp.Application.Interfaces.Repositories;
using ConferenceWebApp.Domain.Enums;

namespace ConferenceWebApp.Infrastructure.Services.Realization.Admin;

public class AdminPaymentService : IAdminPaymentService
{
    private readonly IUserProfileRepository _userProfileRepository;

    public AdminPaymentService(IUserProfileRepository userProfileRepository)
    {
        _userProfileRepository = userProfileRepository;
    }

    public async Task<Result<List<UserWithReceiptDTO>>> GetUsersWithReceiptsAsync()
    {
        try
        {
            var query = _userProfileRepository.GetAllQueryable();

            var users = await query
                .Include(up => up.User)
                .Where(up => !string.IsNullOrEmpty(up.ReceiptFilePath))
                .Select(up => new UserWithReceiptDTO
                {
                    UserId = up.UserId,
                    FullName = $"{up.LastName} {up.FirstName} {up.MiddleName}",
                    Email = up.User.Email,
                    ReceiptFilePath = up.ReceiptFilePath!,
                    HasPaid = (up.Status == ParticipantStatus.CheckSubmitted)
                })
                .ToListAsync();

            return Result<List<UserWithReceiptDTO>>.Success(users);
        }
        catch (Exception ex)
        {
            return Result<List<UserWithReceiptDTO>>.Failure("Ошибка при получении списка пользователей с квитанциями: " + ex.Message);
        }
    }

    public async Task<Result> MarkPaymentAsPaidAsync(Guid userId)
    {
        var profile = await _userProfileRepository.GetByUserIdAsync(userId);

        if (profile == null)
            return Result.Failure("Пользователь не найден.");

        profile.Status = ParticipantStatus.ParticipationConfirmed;
        await _userProfileRepository.UpdateAsync(profile);

        return Result.Success();
    }
}
