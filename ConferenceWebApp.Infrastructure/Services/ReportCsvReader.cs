using ConferenceWebApp.Application.DTOs;
using ConferenceWebApp.Application.Interfaces.Services;
using ConferenceWebApp.Domain.Enums;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConferenceWebApp.Infrastructure.Services;

public sealed class ReportCsvReader:IReportCsvReader
{
    private readonly string _fullPath;
    private readonly char _sep;
    private readonly ILogger<ReportCsvReader> _log;

    public ReportCsvReader (string fullPath, char sep, ILogger<ReportCsvReader> log)
    {
        _fullPath = fullPath;
        _sep = sep;
        _log = log;
    }

    public async Task<IReadOnlyList<ReportCsvRowDTO>> ReadAsync(CancellationToken ct = default)
    {
        if (!File.Exists(_fullPath))
        { throw new FileNotFoundException($"CSV не найден : {_fullPath}"); }

        var rows = new List<ReportCsvRowDTO>();
        using var sr = new StreamReader(_fullPath, Encoding.UTF8);

        string? line;
        int lineNum = 0;
        string[]? header = null;

        while ((line = await sr.ReadLineAsync()) != null)
        {
            ct.ThrowIfCancellationRequested();
            lineNum++;

            if ((string.IsNullOrWhiteSpace(line)) || line.StartsWith('#'))
                continue;

            var parts = SplitCsv(line, _sep).ToArray();

            if (header == null)
            {
                header = parts;
                continue;
            }

            var row = new ReportCsvRowDTO
            {
                Surname = parts[0].Trim(),
                FirstName = parts[1].Trim().NullIfEmpty(),
                MiddleName = parts[2].Trim().NullIfEmpty(),
                EmailsRaw = parts[3].Trim(),
                Title = parts[4].Trim(),
                SectionKey = parts[5].Trim(),
                WorkTypeKey = parts[6].Trim(),
                Fee = int.TryParse(parts[7], NumberStyles.Integer, CultureInfo.InvariantCulture, out var fee) ? fee : 0,
                FileName = parts[8].Trim()
            };

            if (!Enum.TryParse<SectionTopic>(row.SectionKey, true, out _))
            {
                _log.LogWarning($"Строка {line} неизвестный SectionKey {row.SectionKey}' ", lineNum, row.WorkTypeKey);
                continue;
            }

            if (!Enum.TryParse<WorkType>(row.WorkTypeKey, false, out _))
            {
                _log.LogWarning($"Строка {line} неизвестный WorkTypeKey {row.WorkTypeKey}' ", lineNum, row.WorkTypeKey);
                continue;
            }

            rows.Add(row);
        }
        _log.LogInformation("CSV прочитан: {Count} валидных строк из {Path}", rows.Count, _fullPath);
        return rows;
        }


    private static IEnumerable<string> SplitCsv(string s, char sep) 
    {
        var sb =  new StringBuilder();
        bool quoted = false;

        for (int i = 0; i < s.Length; i++)
        {
            char c = s[i];
            if (c == '"')
            {
                if (quoted && i + 1 < s.Length && s[i + 1] == '"')
                {
                    sb.Append('"'); // экранированная кавычка
                    i++;
                }
                else
                {
                    quoted = !quoted;
                }
            }
            else if (c == sep && !quoted)
            {
                yield return sb.ToString();
                sb.Clear();
            }
            else
            {
                sb.Append(c);
            }
        }
        yield return sb.ToString();
    }

  
}
file static class StringExt
{
    public static string? NullIfEmpty(this string s) =>
        string.IsNullOrWhiteSpace(s) ? null : s;
}
