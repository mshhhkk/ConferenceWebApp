using ConferenceWebApp.Application.DTOs.PersonalAccountDTOs;
using Microsoft.AspNetCore.Http;

namespace ConferenceWebApp.Application.Interfaces.Services;

public interface IPaymentService
{
    Task<Result<ReceiptFileDTO>> GetReceiptByUserIdAsync(Guid userId);

    Task<Result> UploadReceiptAsync(Guid userId, IFormFile receipt);

    Task<Result<(Stream FileStream, string ContentType, string FileName)>> DownloadReceiptAsync(Guid userId);
}