namespace ConferenceWebApp.Application.DTOs.ReportsDTOs
{
    public class EditExtendedThesisDTO
    {
        public Guid ReportId { get; set; } 
        public string ReportTheme { get; set; } = string.Empty; 
        public string Organization { get; set; } = string.Empty; 
        public string? ExtThesis { get; set; } 
    }
}