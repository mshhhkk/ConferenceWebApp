using ConferenceWebApp.Domain.Entities;
using ConferenceWebApp.Application.DTOs.PersonalAccountDTOs;
using ConferenceWebApp.Application.ViewModels;
using ConferenceWebApp.Infrastructure.Services.Abstract;
using ConferenceWebApp.Application;
using Microsoft.AspNetCore.Http;
using ConferenceWebApp.Application.Interfaces.Services;
using ConferenceWebApp.Application.Interfaces.Repositories;

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

    public async Task<Result<ReceiptFileViewModel>> GetReceiptAsync(User user)
    {
        var profile = await _userProfileRepository.GetByUserIdAsync(user.Id);
        if (profile == null || !profile.IsRegisteredForConference)
            return Result<ReceiptFileViewModel>.Failure("Вы не зарегистрированы на конференцию.");

        var userProfileDto = new UserProfileDTO
        {
            FullName = $"{profile.FirstName} {profile.LastName} {profile.MiddleName}".Trim(),
            Email = profile.User?.Email ?? string.Empty,
            PhoneNumber = profile.PhoneNumber ?? string.Empty,
            BirthDate = profile.BirthDate,
            Organization = profile.Organization ?? string.Empty,
            Specialization = profile.Specialization ?? string.Empty,
            PhotoUrl = profile.PhotoUrl,
            ParticipantType = profile.ParticipantType,
            HasPaidFee = profile.HasPaidFee,
            IsRegisteredForConference = profile.IsRegisteredForConference,
            IsApprovedAnyReports = profile.HasPaidFee // или другой критерий
        };

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

        var vm = new ReceiptFileViewModel
        {
            UserProfile = userProfileDto,
            ReceiptFile = receiptFileDto
        };

        return Result<ReceiptFileViewModel>.Success(vm);
    }


    public async Task<Result> UploadReceiptAsync(User user, IFormFile receipt)
    {
        var profile = await _userProfileRepository.GetByUserIdAsync(user.Id);
        if (profile == null)
            return Result.Failureure("Профиль пользователя не найден.");

        if (profile.HasPaidFee)
            return Result.Failureure("Оплата уже подтверждена. Загрузка нового чека невозможна.");

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
            await _userProfileRepository.UpdateAsync(profile);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failureure(ex.Message);
        }
    }

    public async Task<Result<(Stream FileStream, string ContentType, string FileName)>> DownloadReceiptAsync(User user)
    {
        try
        {
            var profile = await _userProfileRepository.GetByUserIdAsync(user.Id);
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