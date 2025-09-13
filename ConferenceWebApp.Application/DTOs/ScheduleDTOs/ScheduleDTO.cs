namespace ConferenceWebApp.Application.DTOs.Schedule;

public class ScheduleDTO
{
    public Guid Id { get; set; }
    public DateTime Date { get; set; }
    public string Time { get; set; } = string.Empty;
    public string Event { get; set; } = string.Empty;
}