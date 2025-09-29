using ConferenceWebApp.Application;
using ConferenceWebApp.Application.DTOs.Admin;
using ConferenceWebApp.Application.Interfaces.Repositories;
using ConferenceWebApp.Application.Interfaces.Services.Admin;
using ConferenceWebApp.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ConferenceWebApp.Infrastructure.Services.Realization.Admin;

public class AdminPaymentService : IAdminPaymentService
{
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly ILogger<AdminPaymentService> _logger;

    public AdminPaymentService(
        IUserProfileRepository userProfileRepository,
        ILogger<AdminPaymentService> logger)
    {
        _userProfileRepository = userProfileRepository;
        _logger = logger;
    }

    public async Task<Result<List<UserWithReceiptDTO>>> GetUsersWithReceiptsAsync()
    {
        _logger.LogInformation("Запрос списка пользователей с квитанциями");
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

            _logger.LogInformation("Найдено пользователей с квитанциями: {Count}", users.Count);
            return Result<List<UserWithReceiptDTO>>.Success(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении списка пользователей с квитанциями");
            return Result<List<UserWithReceiptDTO>>.Failure("Ошибка при получении списка пользователей с квитанциями: " + ex.Message);
        }
    }

    public async Task<Result> MarkPaymentAsPaidAsync(Guid userId)
    {
        _logger.LogInformation("Пометка оплаты как подтверждённой для пользователя {UserId}", userId);
        try
        {
            var profile = await _userProfileRepository.GetByUserIdAsync(userId);

            if (profile == null)
            {
                _logger.LogWarning("Профиль пользователя не найден. UserId={UserId}", userId);
                return Result.Failure("Пользователь не найден.");
            }

            profile.Status = ParticipantStatus.ParticipationConfirmed;
            await _userProfileRepository.UpdateAsync(profile);

            _logger.LogInformation("Оплата подтверждена. UserId={UserId}", userId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при пометке оплаты как подтверждённой. UserId={UserId}", userId);
            return Result.Failure("Ошибка при подтверждении оплаты.");
        }
    }
}
