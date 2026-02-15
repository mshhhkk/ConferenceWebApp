using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConferenceWebApp.Application.DTOs;

public sealed class ReportCsvRowDTO
{
    public required string Surname { get; init; }
    public string? FirstName { get; init; }
    public string? MiddleName { get; init; }
    public required string EmailsRaw { get; init; }
    public required string Title { get; init; }
    public required string SectionKey { get; init; }
    public required string WorkTypeKey { get; init; }
    public int Fee { get; init; }
    public required string FileName { get; init; }

    public IEnumerable<string> Emails =>
        EmailsRaw.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                 .Select(e => e.Trim().ToLowerInvariant())
                 .Where(e => e.Length > 0);
}
