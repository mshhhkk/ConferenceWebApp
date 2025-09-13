using ConferenceWebApp.Application.DTOs.AuthDTOs;
using ConferenceWebApp.Application;

namespace ConferenceWebApp.Infrastructure.Services.Abstract;

public interface IAuthService
{
    Task<Result> RegisterAsync(RegisterDTO dto);

    Task<Result> ConfirmEmailAsync(string userId, string token);

    Task<Result> SendTwoFactorCodeAsync(LoginDTO dto);

    Task<Result> VerifyTwoFactorCodeAsync(Verify2FADTO dto);

    Task<Result> LogoutAsync();
}