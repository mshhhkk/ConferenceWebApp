using ConferenceWebApp.Application.DTOs.Schedule;

namespace ConferenceWebApp.Infrastructure.Services.Abstract;

public interface IScheduleService
{
    Task<List<GroupedScheduleDTO>> GetAllGroupedSchedules();
}