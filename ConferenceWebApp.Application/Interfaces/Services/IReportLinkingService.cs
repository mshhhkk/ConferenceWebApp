using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConferenceWebApp.Application.Interfaces.Services;

public interface IReportLinkingService
{
    Task<LinkingResult> BindByEmailAsync(Guid userId, string email, CancellationToken ct = default);
    Task<LinkingResult> BindByNameAsync(Guid userId, string surname, string? firstName, string? middleName, CancellationToken ct = default);
}
public sealed class LinkingResult
{
    public int Added { get; set; }
    public int SkippedAlreadyExists { get; set; }
    public int SkippedInvalid { get; set; }
    public List<string> Messages { get; } = new();
}