// Infrastructure/Services/FileBasedProcessedFilesRegistry.cs
using ConferenceWebApp.Application.Interfaces.Services;
using System.Text.Json;

public sealed class FileBasedProcessedFilesRegistry : IProcessedFilesRegistry
{
    private readonly string _storePath;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private Dictionary<string, Guid> _map = new(StringComparer.OrdinalIgnoreCase);
    private bool _loaded;

    public FileBasedProcessedFilesRegistry(string? storePath = null)
    {
        _storePath = storePath ?? Path.Combine(AppContext.BaseDirectory, "imports", "processed.json");
        var dir = Path.GetDirectoryName(_storePath)!;
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
    }

    public async Task<Guid?> GetOwnerAsync(string normalizedFile, CancellationToken ct = default)
    {
        await EnsureLoadedAsync(ct);
        return _map.TryGetValue(normalizedFile, out var owner) ? owner : (Guid?)null;
    }

    public async Task<bool> TryMarkOwnedAsync(string normalizedFile, Guid ownerUserId, CancellationToken ct = default)
    {
        await _gate.WaitAsync(ct);
        try
        {
            await EnsureLoadedAsync(ct);

            if (_map.TryGetValue(normalizedFile, out var existing))
                return existing == ownerUserId; // уже «моё»

            _map[normalizedFile] = ownerUserId;
            await SaveAsync(ct);
            return true;
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task EnsureLoadedAsync(CancellationToken ct)
    {
        if (_loaded) return;

        if (File.Exists(_storePath))
        {
            var json = await File.ReadAllTextAsync(_storePath, ct);
            var data = JsonSerializer.Deserialize<Dictionary<string, Guid>>(json);
            if (data is not null) _map = new(data, StringComparer.OrdinalIgnoreCase);
        }

        _loaded = true;
    }

    private async Task SaveAsync(CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(_map, new JsonSerializerOptions { WriteIndented = true });
        var tmp = _storePath + ".tmp";
        await File.WriteAllTextAsync(tmp, json, ct);
        File.Move(tmp, _storePath, overwrite: true);
    }
}
