using ConferenceWebApp.Application.DTOs.Schedule;
using ConferenceWebApp.Application.Interfaces.Repositories;
using ConferenceWebApp.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace ConferenceWebApp.Infrastructure.Services.Realization;

public class ScheduleService : IScheduleService
{
    private readonly IScheduleRepository _scheduleRepository;
    private readonly ILogger<ScheduleService> _logger;

    public ScheduleService(IScheduleRepository scheduleRepository, ILogger<ScheduleService> logger)
    {
        _scheduleRepository = scheduleRepository;
        _logger = logger;
    }

    public async Task<List<GroupedScheduleDTO>> GetAllGroupedSchedules()
    {
        _logger.LogInformation("Запрос на получение всех расписаний с группировкой по датам");

        var schedules = await _scheduleRepository.GetAllAsync();

        if (schedules == null || !schedules.Any())
        {
            _logger.LogWarning("Расписания не найдены в базе");
            return new List<GroupedScheduleDTO>();
        }

        var grouped = schedules
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

        _logger.LogInformation("Получено {Count} расписаний, сгруппированных в {Groups} групп",
            schedules.Count, grouped.Count);

        return grouped;
    }
}
