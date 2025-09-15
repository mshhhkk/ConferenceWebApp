using ConferenceWebApp.Application;
using ConferenceWebApp.Application.DTOs.PersonalAccountDTOs;
using ConferenceWebApp.Application.Interfaces.Repositories;
using ConferenceWebApp.Application.Interfaces.Services;
using ConferenceWebApp.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace ConferenceWebApp.Infrastructure.Services.Realization;

public class PaymentService : IPaymentService
{
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly IFileService _fileService;

    public PaymentService(IUserProfileRepository userProfileRepository, IFileService fileService)
    {
        _userProfileRepository = userProfileRepository;
        _fileService = fileService;
    }

    public async Task<Result<ReceiptFileDTO>> GetReceiptByUserIdAsync(Guid userId)
    {
        var profile = await _userProfileRepository.GetByUserIdAsync(userId);
        if (profile == null || profile.Status == ParticipantStatus.ProfileCompleted)
            return Result<ReceiptFileDTO>.Failure("Вы не зарегистрированы на конференцию.");

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

        return Result<ReceiptFileDTO>.Success(receiptFileDto);
    }


    public async Task<Result> UploadReceiptAsync(Guid userId, IFormFile receipt)
    {
        var profile = await _userProfileRepository.GetByUserIdAsync(userId);

        if (profile.Status == ParticipantStatus.ParticipationConfirmed)
            return Result.Failure("Оплата уже подтверждена. Загрузка нового чека невозможна.");

        var allowedContentTypes = new[] { "image/jpeg", "image/png", "application/pdf" };
        const long maxSize = 10 * 1024 * 1024;

        try
        {
            if (!string.IsNullOrEmpty(profile.ReceiptFilePath))
            {
                _fileService.DeleteFile(profile.ReceiptFilePath);
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

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message);
        }
    }

    public async Task<Result<(Stream FileStream, string ContentType, string FileName)>> DownloadReceiptAsync(Guid userId)
    {
        try
        {
            var profile = await _userProfileRepository.GetByUserIdAsync(userId);
            if (profile == null || string.IsNullOrEmpty(profile.ReceiptFilePath))
                return Result<(Stream, string, string)>.Failure("Чек не найден.");

            var fileResult = await _fileService.GetFileAsync(profile.ReceiptFilePath);
            return Result<(Stream, string, string)>.Success(fileResult);
        }
        catch (Exception ex)
        {
            return Result<(Stream, string, string)>.Failure(ex.Message);
        }
    }
}