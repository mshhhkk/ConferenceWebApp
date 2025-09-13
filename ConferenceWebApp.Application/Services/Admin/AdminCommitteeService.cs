using ConferenceWebApp.Domain.Entities;
using ConferenceWebApp.Application.Interfaces.Repositories;
using ConferenceWebApp.Application.DTOs.Admin;
using ConferenceWebApp.Infrastructure.Services.Abstract.Admin;
using Microsoft.AspNetCore.Http;

public class AdminCommitteeService : IAdminCommitteeService
{
    private readonly ICommitteRepository _committeeRepository;

    public AdminCommitteeService(ICommitteRepository committeeRepository)
    {
        _committeeRepository = committeeRepository;
    }

    public async Task<List<AdminCommitteeDTO>> GetAllCommitteesAsync()
    {
        var committees = await _committeeRepository.GetAllAsync();
        return committees.Select(c => new AdminCommitteeDTO
        {
            Id = c.Id,
            FullName = c.FullName,
            Description = c.Description,
            PhotoUrl = c.PhotoUrl,
            IsHead = c.IsHead
        }).ToList();
    }

    public async Task<AdminCommitteeDTO?> GetCommitteeByIdAsync(Guid id)
    {
        var c = await _committeeRepository.GetByIdAsync(id);
        if (c == null) return null;

        return new AdminCommitteeDTO
        {
            Id = c.Id,
            FullName = c.FullName,
            Description = c.Description,
            PhotoUrl = c.PhotoUrl,
            IsHead = c.IsHead
        };
    }

    public async Task AddCommitteeAsync(AdminCommitteeDTO dto, IFormFile? photo)
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
    }

    public async Task<bool> EditCommitteeAsync(AdminCommitteeDTO dto, IFormFile? photo)
    {
        var committee = await _committeeRepository.GetByIdAsync(dto.Id);
        if (committee == null) return false;

        committee.FullName = dto.FullName;
        committee.Description = dto.Description;
        committee.IsHead = dto.IsHead;

        if (photo != null)
        {
            committee.PhotoUrl = await CommitteePhotoAsync(photo);
        }

        await _committeeRepository.UpdateAsync(committee);
        return true;
    }

    public async Task DeleteCommitteeAsync(Guid id)
    {
        await _committeeRepository.DeleteAsync(id);
    }

    private async Task<string> CommitteePhotoAsync(IFormFile photo)
    {
        if (photo == null || photo.Length == 0)
            return "/committee/defaultUserPhoto.png";
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(photo.FileName)}";
        var filePath = Path.Combine("wwwroot/committee", fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {   
            await photo.CopyToAsync(stream);
        }

        return $"/comittee/{fileName}";
    }
}