using ConferenceWebApp.Domain.Entities;
using ConferenceWebApp.Application.ViewModels;
using ConferenceWebApp.Application;
using Microsoft.AspNetCore.Http;
using ConferenceWebApp.Application.DTOs.PersonalAccountDTOs;

namespace ConferenceWebApp.Infrastructure.Services.Abstract;

public interface IPaymentService
{
    Task<Result<ReceiptFileDTO>> GetReceiptByUserIdAsync(Guid userId);

    Task<Result> UploadReceiptAsync(Guid userId, IFormFile receipt);

    Task<Result<(Stream FileStream, string ContentType, string FileName)>> DownloadReceiptAsync(Guid userId);
}