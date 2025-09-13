using ConferenceWebApp.Domain.Entities;
using ConferenceWebApp.Application.ViewModels;
using ConferenceWebApp.Application;
using Microsoft.AspNetCore.Http;

namespace ConferenceWebApp.Infrastructure.Services.Abstract;

public interface IPaymentService
{
    Task<Result<ReceiptFileViewModel>> GetReceiptAsync(User user);

    Task<Result> UploadReceiptAsync(User user, IFormFile receipt);

    Task<Result<(Stream FileStream, string ContentType, string FileName)>> DownloadReceiptAsync(User user);
}