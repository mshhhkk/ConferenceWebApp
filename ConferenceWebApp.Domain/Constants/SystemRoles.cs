namespace ConferenceWebApp.Domain.Constants;

public static class SystemRoles
{
    public const string Participant = "Participant";
    public const string Admin = "Admin";
    public const string SuperAdmin = "SuperAdmin";

    public static readonly Guid ParticipantId = Guid.Parse("375FA642-7E6A-4333-B78F-270ED825997F");
    public static readonly Guid AdminId = Guid.Parse("6282CBD6-908D-4E0E-A3EE-6F41CD54FB5F");
    public static readonly Guid SuperAdminId = Guid.Parse("20AEDC71-9FC5-4272-B766-E6DC1AEB63AE");
}
