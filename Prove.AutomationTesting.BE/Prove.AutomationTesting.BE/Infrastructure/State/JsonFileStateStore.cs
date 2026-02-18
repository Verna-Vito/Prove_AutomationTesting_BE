using System.IO;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ApiTests.Infrastructure.State;

/// <summary>
/// Store JSON su singolo file (RunState.json).
///
/// Caratteristiche:
/// - un solo file
/// - lock cross-platform con timeout 30s
/// - scrittura atomica (tmp -> move/replace) per evitare file corrotti
/// - UpdateAsync: carica-modifica-salva sotto lo stesso lock (riduce race)
/// </summary>
public sealed class JsonFileStateStore
{
    private static readonly TimeSpan LockTimeout = TimeSpan.FromSeconds(30);

    private readonly string _path;
    private readonly string _lockPath;
    private readonly SemaphoreSlim _gate = new(1, 1);

    private readonly JsonSerializerOptions _jsonOptions =
        new() { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public JsonFileStateStore(string path)
    {
        _path = Path.GetFullPath(path);
        _lockPath = _path + ".lock";
    }

    public async Task<RunState> LoadAsync(CancellationToken ct = default)
    {
        await _gate.WaitAsync(ct);
        try
        {
            await using var _ = await FileLock.AcquireAsync(_lockPath, LockTimeout, ct);

            if (!File.Exists(_path))
                return new RunState();

            var json = await File.ReadAllTextAsync(_path, ct);
            if (string.IsNullOrWhiteSpace(json))
                return new RunState();

            return JsonSerializer.Deserialize<RunState>(json, _jsonOptions) ?? new RunState();
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task SaveAsync(RunState state, CancellationToken ct = default)
    {
        await _gate.WaitAsync(ct);
        try
        {
            await using var _ = await FileLock.AcquireAsync(_lockPath, LockTimeout, ct);
            await SaveInternalAsync(state, ct);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task UpdateAsync(Action<RunState> mutate, CancellationToken ct = default)
    {
        await _gate.WaitAsync(ct);
        try
        {
            await using var _ = await FileLock.AcquireAsync(_lockPath, LockTimeout, ct);

            RunState state;
            if (File.Exists(_path))
            {
                var json = await File.ReadAllTextAsync(_path, ct);
                state = JsonSerializer.Deserialize<RunState>(json, _jsonOptions) ?? new RunState();
            }
            else
            {
                state = new RunState();
            }

            mutate(state);

            await SaveInternalAsync(state, ct);
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task SaveInternalAsync(RunState state, CancellationToken ct)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);

        state.UpdatedAtUtc = DateTime.UtcNow.ToString("O");

        var json = JsonSerializer.Serialize(state, _jsonOptions);

        // tmp nella stessa directory: move/rename affidabile e “quasi atomico”
        var tmp = _path + ".tmp";
        await File.WriteAllTextAsync(tmp, json, ct);

        // Windows: File.Replace (molto robusto se target esiste)
        // Linux/macOS: rename/move è atomico, e overwrite true va bene.
        if (OperatingSystem.IsWindows() && File.Exists(_path))
            File.Replace(tmp, _path, null);
        else
            File.Move(tmp, _path, overwrite: true);
    }
}
