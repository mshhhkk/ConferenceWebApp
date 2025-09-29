using System.Security.Cryptography;
using ConferenceWebApp.Application.Interfaces.Repositories;
using ConferenceWebApp.Application.Interfaces.Services;
using ConferenceWebApp.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace ConferenceWebApp.Infrastructure.Services.Realization
{
    public class TwoFactorService : ITwoFactorService
    {
        private readonly ITwoFactorCodeRepository _twoFactorCodeRepository;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<TwoFactorService> _logger;

        public TwoFactorService(
            ITwoFactorCodeRepository twoFactorCodeRepository,
            UserManager<User> userManager,
            ILogger<TwoFactorService> logger)
        {
            _twoFactorCodeRepository = twoFactorCodeRepository;
            _userManager = userManager;
            _logger = logger;
        }

        public string GenerateCode()
        {
            var number = 111111;

            // Если нужно включить реальную генерацию:
            // using var rng = RandomNumberGenerator.Create();
            // var bytes = new byte[4];
            // rng.GetBytes(bytes);
            // var number = BitConverter.ToUInt32(bytes, 0) % 1_000_000;

            var code = number.ToString("D6");
            _logger.LogInformation("Сгенерирован код двухфакторной аутентификации: {Code}", code);
            return code;
        }

        public async Task StoreCodeAsync(string email, string code)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                _logger.LogWarning("Попытка сохранить код для несуществующего пользователя {Email}", email);
                throw new Exception("Пользователь не найден");
            }

            var twoFactorCode = new TwoFactorCode
            {
                UserId = user.Id,
                Code = code,
                ExpirationTime = DateTime.UtcNow.AddMinutes(3)
            };

            await _twoFactorCodeRepository.AddAsync(twoFactorCode);
            _logger.LogInformation("Код для пользователя {Email} сохранён и будет действителен до {Expiration}", email, twoFactorCode.ExpirationTime);
        }

        public async Task<bool> ValidateCodeAsync(string email, string inputCode)
        {
            var twoFactorCode = await _twoFactorCodeRepository.GetLatestByEmailAsync(email);

            if (twoFactorCode == null)
            {
                _logger.LogWarning("Для пользователя {Email} не найдено кодов", email);
                return false;
            }

            if (twoFactorCode.ExpirationTime < DateTime.UtcNow)
            {
                _logger.LogWarning("Код для пользователя {Email} истёк ({Expiration})", email, twoFactorCode.ExpirationTime);
                return false;
            }

            if (twoFactorCode.Code != inputCode)
            {
                _logger.LogWarning("Неверный код для пользователя {Email}: введено {Input}, ожидалось {Expected}", email, inputCode, twoFactorCode.Code);
                return false;
            }

            await _twoFactorCodeRepository.RemoveAsync(twoFactorCode);
            _logger.LogInformation("Код для пользователя {Email} подтверждён и удалён из базы", email);

            return true;
        }
    }
}
