using System.ComponentModel.DataAnnotations;

namespace ConferenceWebApp.Domain.Entities;

public class Schedule
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public DateOnly Date { get; set; }

    [Required]
    [DataType(DataType.Time)]
    public TimeSpan StartingTime { get; set; }

    [Required]
    [DataType(DataType.Time)]
    public TimeSpan EndingTime { get; set; }

    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
}