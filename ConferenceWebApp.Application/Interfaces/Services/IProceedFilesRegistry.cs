using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConferenceWebApp.Application.Interfaces.Services;

public interface IProcessedFilesRegistry
{
    Task<Guid?> GetOwnerAsync(string normalizedFile, CancellationToken ct = default);
    Task<bool> TryMarkOwnedAsync(string normalizedFile, Guid ownerUserId, CancellationToken ct = default);
}