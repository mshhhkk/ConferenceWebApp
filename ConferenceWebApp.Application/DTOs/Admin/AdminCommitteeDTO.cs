namespace ConferenceWebApp.Application.DTOs.Admin
{
    public class AdminCommitteeDTO
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string PhotoUrl { get; set; } = string.Empty;
        public bool IsHead { get; set; }
    }
}