// Application/DTOs/PersonalAccountDTOs/InvitationDTO.cs
namespace ConferenceWebApp.Application.DTOs.PersonalAccountDTOs;

public sealed class InvitationDTO
{
    public string GreetingName { get; set; } = default!;     
    public string FullName { get; set; } = default!;        
    public string ConferenceName { get; set; } = "Нанофизика и наноэлектроника (КЭ'2025)";
    public string Dates { get; set; } = "18–20 ноября 2025 г.";
    public string Venue { get; set; } =
        "Белорусский государственный университет, НИИ прикладных физических проблем им. А.Н. Севченко БГУ, Минск, ул. Курчатова, 7.";
    public bool IsStudent { get; set; }                     
    public List<InvitationReportDTO> Reports { get; set; } = new();
}

public sealed class InvitationReportDTO
{
    public string Title { get; set; } = default!;
    public string WorkTypeText { get; set; } = default!;    
    public string SectionText { get; set; } = default!;  
}
