using ConferenceWebApp.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConferenceWebApp.Application.DTOs.Admin;

public class AdminEditUserDTO
{
    public Guid UserId { get; set; }
    public string? FirstName { get; set; } = string.Empty;

    public string? LastName { get; set; } = string.Empty;

    public string? MiddleName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public DateOnly BirthDate { get; set; }
    public string Organization { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;

    public ParticipantType ParticipantType { get; set; }
    public ParticipantStatus Status { get; set; }
    public UserApprovalStatus ApprovalStatus { get; set; }
    public ScientificDegree Degree { get; set; }
    public Position Position { get; set; }

}
