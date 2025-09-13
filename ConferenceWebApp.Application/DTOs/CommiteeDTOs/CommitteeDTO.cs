namespace ConferenceWebApp.Application.DTOs.CommiteeDTOs
{
    public class CommitteeDTO
    {
        public string FullName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string PhotoUrl { get; set; } = string.Empty;
        public bool IsHead { get; set; }
    }
}