namespace ConferenceWebApp.Infrastructure.Services.Abstract;

public interface ITwoFactorService
{
    string GenerateCode();

    Task StoreCodeAsync(string email, string code);

    Task<bool> ValidateCodeAsync(string email, string inputCode);
}