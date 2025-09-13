using ConferenceWebApp.Application.DTOs.Schedule;
using ConferenceWebApp.Infrastructure.Services.Abstract;
using ConferenceWebApp.Application.Interfaces.Repositories;

namespace ConferenceWebApp.Infrastructure.Services.Realization;

public class ScheduleService : IScheduleService
{
    private readonly IScheduleRepository _scheduleRepository;

    public ScheduleService(IScheduleRepository scheduleRepository)
    {
        _scheduleRepository = scheduleRepository;
    }

    public async Task<List<GroupedScheduleDTO>> GetAllGroupedSchedules()
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
}