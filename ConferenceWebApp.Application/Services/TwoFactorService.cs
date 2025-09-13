using System.Security.Cryptography;
using ConferenceWebApp.Domain.Entities;
using ConferenceWebApp.Infrastructure.Services.Abstract;
using Microsoft.AspNetCore.Identity;
using ConferenceWebApp.Application.Interfaces.Repositories;

namespace ConferenceWebApp.Infrastructure.Services.Realization
{
    public class TwoFactorService : ITwoFactorService
    {
        private readonly ITwoFactorCodeRepository _twoFactorCodeRepository;
        private readonly UserManager<User> _userManager;

        public TwoFactorService(ITwoFactorCodeRepository twoFactorCodeRepository, UserManager<User> userManager)
        {
            _twoFactorCodeRepository = twoFactorCodeRepository;
            _userManager = userManager;
        }

        public string GenerateCode()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[4];
            rng.GetBytes(bytes);
            //var number = BitConverter.ToUInt32(bytes, 0) % 1_000_000;
            var number = 111111; // Для тестов, чтобы не генерировать каждый раз новый код
            return number.ToString("D6"); // всегда 6 цифр
        }

        public async Task StoreCodeAsync(string email, string code)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                throw new Exception("Пользователь не найден");

            var twoFactorCode = new TwoFactorCode
            {
                UserId = user.Id,
                Code = code,
                ExpirationTime = DateTime.UtcNow.AddMinutes(3) // Код действует 3 минуты
            };

            await _twoFactorCodeRepository.AddAsync(twoFactorCode);
        }

        public async Task<bool> ValidateCodeAsync(string email, string inputCode)
        {
            var twoFactorCode = await _twoFactorCodeRepository.GetLatestByEmailAsync(email);
            if (twoFactorCode == null || twoFactorCode.ExpirationTime < DateTime.UtcNow || twoFactorCode.Code != inputCode)
                return false;

            // Удаляем код после успешной проверки
            await _twoFactorCodeRepository.RemoveAsync(twoFactorCode);
            return true;
        }
    }
}