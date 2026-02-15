using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConferenceWebApp.Application.DTOs;
namespace ConferenceWebApp.Application.Interfaces.Services;

public interface IReportCsvReader
{
    Task<IReadOnlyList<ReportCsvRowDTO>> ReadAsync(CancellationToken ct = default);
}
