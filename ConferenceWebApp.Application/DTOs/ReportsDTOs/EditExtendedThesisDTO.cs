namespace ConferenceWebApp.Application.DTOs.ReportsDTOs
{
    public class EditExtendedThesisDTO
    {
        public Guid ReportId { get; set; } // Идентификатор доклада
        public string ReportTheme { get; set; } = string.Empty; // Тема доклада (неизменяемое)
        public string Organization { get; set; } = string.Empty; // Организация пользователя (неизменяемое)
        public string? ExtThesis { get; set; } // Расширенные тезисы (редактируемое поле)
    }
}