using ConferenceWebApp.Application.DTOs.Admin;
using ConferenceWebApp.Application.DTOs.Schedule;

namespace ConferenceWebApp.Infrastructure.Services.Abstract.Admin;

public interface IScheduleAdminService
{
    Task<List<GroupedScheduleDTO>> GetGroupedScheduleAsync();

    Task AddScheduleAsync(AdminScheduleDTO dto);

    Task<AdminScheduleDTO?> GetScheduleForEditAsync(Guid id);

    Task<bool> UpdateScheduleAsync(AdminScheduleDTO dto);

    Task DeleteScheduleAsync(Guid id);
}