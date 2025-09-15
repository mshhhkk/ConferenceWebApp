using ConferenceWebApp.Application.DTOs.AuthDTOs;

namespace ConferenceWebApp.Application.Interfaces.Services;

public interface IAuthService
{
    Task<Result> RegisterAsync(RegisterDTO dto);

    Task<Result> ConfirmEmailAsync(string userId, string token);

    Task<Result> SendTwoStepCodeAsync(LoginDTO dto);

    Task<Result> VerifyTwoFactorStepsAsync(Verify2FADTO dto);

    Task<Result> LogoutAsync();
}