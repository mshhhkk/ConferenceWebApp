using ConferenceWebApp.Application.DTOs.Schedule;

namespace ConferenceWebApp.Application.Interfaces.Services;

public interface IScheduleService
{
    Task<List<GroupedScheduleDTO>> GetAllGroupedSchedules();
}