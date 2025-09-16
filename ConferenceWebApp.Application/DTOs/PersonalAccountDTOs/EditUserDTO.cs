using ConferenceWebApp.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace ConferenceWebApp.Application.DTOs.PersonalAccountDTOs;

public class EditUserDTO
{
    public string? FirstName { get; set; } = string.Empty;

    public string? LastName { get; set; } = string.Empty;

    public string? MiddleName { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; } = string.Empty;

    public DateOnly? BirthDate { get; set; }

    public string? Organization { get; set; } = string.Empty;

    public string? Specialization { get; set; } = string.Empty;

    public string? PhotoUrl { get; set; }

    public IFormFile? Photo { get; set; }

    public bool RemovePhoto { get; set; } = false;

    public Position? Position { get; set; }

    public ScientificDegree? Degree { get; set; }

    public bool IsRegisteredForConference =>
        !string.IsNullOrEmpty(FirstName) &&
        !string.IsNullOrEmpty(LastName) &&
        !string.IsNullOrEmpty(MiddleName) &&
        !string.IsNullOrEmpty(Organization) &&
        !string.IsNullOrEmpty(Specialization) &&
        !string.IsNullOrEmpty(PhoneNumber);


}
