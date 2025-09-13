using System.ComponentModel.DataAnnotations;

namespace ConferenceWebApp.Application.DTOs.Admin;

public class AdminScheduleDTO
{
    public Guid Id { get; set; } // Добавлено для редактирования

    [Required(ErrorMessage = "Укажите дату")]
    [Display(Name = "Дата")]
    public DateOnly Date { get; set; }

    [Required(ErrorMessage = "Укажите время начала")]
    [DataType(DataType.Time)]
    [Display(Name = "Время начала")]
    public TimeSpan StartingTime { get; set; }

    [Required(ErrorMessage = "Укажите время окончания")]
    [DataType(DataType.Time)]
    [Display(Name = "Время окончания")]
    public TimeSpan EndingTime { get; set; }

    [Required(ErrorMessage = "Введите описание")]
    [MaxLength(500, ErrorMessage = "Описание не должно превышать 500 символов")]
    [Display(Name = "Описание")]
    public string Description { get; set; } = string.Empty;
}