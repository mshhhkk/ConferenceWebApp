namespace ConferenceWebApp.Application.DTOs.Schedule
{
    public class GroupedScheduleDTO
    {
        public DateTime Date { get; set; }
        public List<ScheduleDTO> Events { get; set; } = new();
    }
}