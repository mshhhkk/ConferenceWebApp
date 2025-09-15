using ConferenceWebApp.Application.DTOs.Admin;
using ConferenceWebApp.Application.DTOs.Schedule;
using ConferenceWebApp.Application.Interfaces.Repositories;
using ConferenceWebApp.Application.Interfaces.Services.Admin;
using ConferenceWebApp.Domain.Entities;

namespace ConferenceWebApp.Infrastructure.Services.Realization.Admin;

public class ScheduleAdminService : IScheduleAdminService
{
    private readonly IScheduleRepository _scheduleRepository;

    public ScheduleAdminService(IScheduleRepository scheduleRepository)
    {
        _scheduleRepository = scheduleRepository;
    }

    public async Task<List<GroupedScheduleDTO>> GetGroupedScheduleAsync()
    {
        var schedules = await _scheduleRepository.GetAllAsync();
        return schedules
            .GroupBy(s => s.Date)
            .Select(g => new GroupedScheduleDTO
            {
                Date = g.Key.ToDateTime(TimeOnly.MinValue),
                Events = g.Select(e => new ScheduleDTO
                {
                    Id = e.Id,
                    Time = $"{e.StartingTime:hh\\:mm} - {e.EndingTime:hh\\:mm}",
                    Event = e.Description
                }).ToList()
            }).ToList();
    }

    public async Task AddScheduleAsync(AdminScheduleDTO dto)
    {
        var schedule = new Schedule
        {
            Id = Guid.NewGuid(),
            Date = dto.Date,
            StartingTime = dto.StartingTime,
            EndingTime = dto.EndingTime,
            Description = dto.Description
        };

        await _scheduleRepository.AddAsync(schedule);
    }

    public async Task<AdminScheduleDTO?> GetScheduleForEditAsync(Guid id)
    {
        var schedule = await _scheduleRepository.GetByIdAsync(id);
        if (schedule == null) return null;

        return new AdminScheduleDTO
        {
            Id = schedule.Id,
            Date = schedule.Date,
            StartingTime = schedule.StartingTime,
            EndingTime = schedule.EndingTime,
            Description = schedule.Description
        };
    }

    public async Task<bool> UpdateScheduleAsync(AdminScheduleDTO dto)
    {
        var schedule = await _scheduleRepository.GetByIdAsync(dto.Id);
        if (schedule == null) return false;

        schedule.Date = dto.Date;
        schedule.StartingTime = dto.StartingTime;
        schedule.EndingTime = dto.EndingTime;
        schedule.Description = dto.Description;

        await _scheduleRepository.UpdateAsync(schedule);
        return true;
    }

    public async Task DeleteScheduleAsync(Guid id)
    {
        await _scheduleRepository.DeleteAsync(id);
    }
}