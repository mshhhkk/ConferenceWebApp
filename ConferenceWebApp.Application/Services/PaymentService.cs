using ConferenceWebApp.Application;
using ConferenceWebApp.Application.DTOs.PersonalAccountDTOs;
using ConferenceWebApp.Application.Interfaces.Repositories;
using ConferenceWebApp.Application.Interfaces.Services;
using ConferenceWebApp.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ConferenceWebApp.Infrastructure.Services.Realization;

public class PaymentService : IPaymentService
{
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly IFileService _fileService;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        IUserProfileRepository userProfileRepository,
        IFileService fileService,
        ILogger<PaymentService> logger)
    {
        _userProfileRepository = userProfileRepository;
        _fileService = fileService;
        _logger = logger;
    }

    public async Task<Result<ReceiptFileDTO>> GetReceiptByUserIdAsync(Guid userId)
    {
        try
        {
            _logger.LogInformation("Получение чека пользователя {UserId}", userId);

            var profile = await _userProfileRepository.GetByUserIdAsync(userId);
            if (profile == null || profile.Status == ParticipantStatus.ProfileCompleted)
            {
                _logger.LogWarning("Профиль пользователя {UserId} не найден или статус ProfileCompleted", userId);
                return Result<ReceiptFileDTO>.Failure("Вы не зарегистрированы на конференцию.");
            }

            ReceiptFileDTO? receiptFileDto = null;
            if (!string.IsNullOrEmpty(profile.ReceiptFilePath))
            {
                var fileMetadata = await _fileService.TryGetFileMetadataAsync(profile.ReceiptFilePath);
                if (fileMetadata.HasValue)
                {
                    receiptFileDto = new ReceiptFileDTO
                    {
                        FileName = fileMetadata.Value.FileName,
                        UploadDate = fileMetadata.Value.UploadDate
                    };
                }
            }

            _logger.LogInformation("Чек пользователя {UserId} успешно получен", userId);
            return Result<ReceiptFileDTO>.Success(receiptFileDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении чека пользователя {UserId}", userId);
            return Result<ReceiptFileDTO>.Failure("Ошибка при загрузке данных чека.");
        }
    }

    public async Task<Result> UploadReceiptAsync(Guid userId, IFormFile receipt)
    {
        try
        {
            _logger.LogInformation("Загрузка чека для пользователя {UserId}", userId);

            var profile = await _userProfileRepository.GetByUserIdAsync(userId);
            if (profile.Status == ParticipantStatus.ParticipationConfirmed)
            {
                _logger.LogWarning("Пользователь {UserId} пытается загрузить чек, но участие уже подтверждено", userId);
                return Result.Failure("Оплата уже подтверждена. Загрузка нового чека невозможна.");
            }

            var allowedContentTypes = new[] { "image/jpeg", "image/png", "application/pdf" };
            const long maxSize = 10 * 1024 * 1024;

            if (!string.IsNullOrEmpty(profile.ReceiptFilePath))
            {
                _fileService.DeleteFile(profile.ReceiptFilePath);
                _logger.LogInformation("Удалён старый чек пользователя {UserId}", userId);
            }

            var filePath = await _fileService.UpdateFileAsync(
                receipt,
                profile.ReceiptFilePath,
                "receipts",
                allowedContentTypes,
                maxSize
            );

            profile.ReceiptFilePath = filePath;
            profile.Status = ParticipantStatus.CheckSubmitted;
            await _userProfileRepository.UpdateAsync(profile);

            _logger.LogInformation("Чек пользователя {UserId} успешно загружен", userId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при загрузке чека пользователя {UserId}", userId);
            return Result.Failure("Ошибка при загрузке чека.");
        }
    }

    public async Task<Result<(Stream FileStream, string ContentType, string FileName)>> DownloadReceiptAsync(Guid userId)
    {
        try
        {
            _logger.LogInformation("Загрузка файла чека для пользователя {UserId}", userId);

            var profile = await _userProfileRepository.GetByUserIdAsync(userId);
            if (profile == null || string.IsNullOrEmpty(profile.ReceiptFilePath))
            {
                _logger.LogWarning("Чек не найден для пользователя {UserId}", userId);
                return Result<(Stream, string, string)>.Failure("Чек не найден.");
            }

            var fileResult = await _fileService.GetFileAsync(profile.ReceiptFilePath);

            _logger.LogInformation("Файл чека успешно загружен для пользователя {UserId}", userId);
            return Result<(Stream, string, string)>.Success(fileResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при скачивании чека пользователя {UserId}", userId);
            return Result<(Stream, string, string)>.Failure("Ошибка при скачивании чека.");
        }
    }
}
