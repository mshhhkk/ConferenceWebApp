using ConferenceWebApp.Domain.Entities;

namespace ConferenceWebApp.Application.DTOs.ReportsDTOs;

public class ExtendedThesisDTO
{
    public string Organization { get; set; } = string.Empty;
    public List<Reports> ReportsWithTheses { get; set; } = new();
    public List<Reports> ReportsWithoutTheses { get; set; } = new();
}