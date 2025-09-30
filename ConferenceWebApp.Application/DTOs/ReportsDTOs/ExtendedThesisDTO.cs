using ConferenceWebApp.Domain.Entities;

namespace ConferenceWebApp.Application.DTOs.ReportsDTOs;

public class ExtendedThesisDTO
{
    public string Organization { get; set; } = string.Empty;
    public List<ReportDTO> ReportsWithTheses { get; set; } = new();
    public List<ReportDTO> ReportsWithoutTheses { get; set; } = new();
}