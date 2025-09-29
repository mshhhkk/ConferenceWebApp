using ConferenceWebApp.Application.DTOs.Admin;
using ConferenceWebApp.Application.Interfaces.Repositories;
using ConferenceWebApp.Application.Interfaces.Services.Admin;
using ConferenceWebApp.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

public class AdminCommitteeService : IAdminCommitteeService
{
    private readonly ICommitteRepository _committeeRepository;
    private readonly ILogger<AdminCommitteeService> _logger;

    public AdminCommitteeService(
        ICommitteRepository committeeRepository,
        ILogger<AdminCommitteeService> logger)
    {
        _committeeRepository = committeeRepository;
        _logger = logger;
    }

    public async Task<List<AdminCommitteeDTO>> GetAllCommitteesAsync()
    {
        _logger.LogInformation("Запрос списка комитетов");
        try
        {
            var committees = await _committeeRepository.GetAllAsync();
            var result = committees.Select(c => new AdminCommitteeDTO
            {
                Id = c.Id,
                FullName = c.FullName,
                Description = c.Description,
                PhotoUrl = c.PhotoUrl,
                IsHead = c.IsHead
            }).ToList();

            _logger.LogInformation("Получено {Count} записей комитета", result.Count);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении списка комитетов");
            throw;
        }
    }

    public async Task<AdminCommitteeDTO?> GetCommitteeByIdAsync(Guid id)
    {
        _logger.LogInformation("Запрос комитета по Id={Id}", id);
        try
        {
            var c = await _committeeRepository.GetByIdAsync(id);
            if (c == null)
            {
                _logger.LogWarning("Комитет не найден. Id={Id}", id);
                return null;
            }

            return new AdminCommitteeDTO
            {
                Id = c.Id,
                FullName = c.FullName,
                Description = c.Description,
                PhotoUrl = c.PhotoUrl,
                IsHead = c.IsHead
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении комитета Id={Id}", id);
            throw;
        }
    }

    public async Task AddCommitteeAsync(AdminCommitteeDTO dto, IFormFile? photo)
    {
        _logger.LogInformation("Добавление комитета: {FullName}", dto?.FullName);
        try
        {
            var newCommittee = new Committee
            {
                Id = Guid.NewGuid(),
                FullName = dto.FullName,
                Description = dto.Description,
                IsHead = dto.IsHead,
                PhotoUrl = photo != null ? await CommitteePhotoAsync(photo) : string.Empty
            };

            await _committeeRepository.AddAsync(newCommittee);
            _logger.LogInformation("Комитет добавлен. Id={Id}", newCommittee.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при добавлении комитета: {FullName}", dto?.FullName);
            throw;
        }
    }

    public async Task<bool> EditCommitteeAsync(AdminCommitteeDTO dto, IFormFile? photo)
    {
        _logger.LogInformation("Редактирование комитета Id={Id}", dto.Id);
        try
        {
            var committee = await _committeeRepository.GetByIdAsync(dto.Id);
            if (committee == null)
            {
                _logger.LogWarning("Не найден комитет для редактирования. Id={Id}", dto.Id);
                return false;
            }

            committee.FullName = dto.FullName;
            committee.Description = dto.Description;
            committee.IsHead = dto.IsHead;

            if (photo != null)
            {
                committee.PhotoUrl = await CommitteePhotoAsync(photo);
            }

            await _committeeRepository.UpdateAsync(committee);
            _logger.LogInformation("Комитет обновлён. Id={Id}", dto.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при редактировании комитета Id={Id}", dto.Id);
            throw;
        }
    }

    public async Task DeleteCommitteeAsync(Guid id)
    {
        _logger.LogInformation("Удаление комитета Id={Id}", id);
        try
        {
            await _committeeRepository.DeleteAsync(id);
            _logger.LogInformation("Комитет удалён Id={Id}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при удалении комитета Id={Id}", id);
            throw;
        }
    }

    private async Task<string> CommitteePhotoAsync(IFormFile photo)
    {
        _logger.LogInformation("Сохранение фото комитета. Имя файла: {Name}, Размер: {Length}", photo.FileName, photo.Length);
        try
        {
            if (photo == null || photo.Length == 0)
            {
                _logger.LogWarning("Пустой файл фото комитета, возвращаем дефолт");
                return "/committee/defaultUserPhoto.png";
            }

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(photo.FileName)}";
            var dir = Path.Combine("wwwroot", "committee");
            Directory.CreateDirectory(dir);

            var filePath = Path.Combine(dir, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await photo.CopyToAsync(stream);
            }

            var url = $"/committee/{fileName}";
            _logger.LogInformation("Фото сохранено: {Path} (url: {Url})", filePath, url);
            return url;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при сохранении фото комитета");
            throw;
        }
    }
}
