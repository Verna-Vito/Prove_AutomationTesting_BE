using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Threading;

namespace ApiTests.Infrastructure.State;

/// <summary>
/// Lock cross-platform basato su file.
/// Meccanismo:
/// - apre un file .lock in esclusiva (FileShare.None)
/// - se già aperto da un altro test, IOException e si ritenta
///
/// Con timeout: niente deadlock infinito.
/// </summary>
public sealed class FileLock : IAsyncDisposable
{
    private readonly FileStream _stream;

    private FileLock(FileStream stream) => _stream = stream;

    public static async Task<FileLock> AcquireAsync(
        string lockPath,
        TimeSpan timeout,
        CancellationToken ct = default)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(lockPath))!);

        var sw = Stopwatch.StartNew();
        Exception? last = null;

        while (sw.Elapsed < timeout)
        {
            ct.ThrowIfCancellationRequested();

            try
            {
                var fs = new FileStream(
                    lockPath,
                    FileMode.OpenOrCreate,
                    FileAccess.ReadWrite,
                    FileShare.None);

                return new FileLock(fs);
            }
            catch (IOException ex)
            {
                last = ex;
                await Task.Delay(50, ct);
            }
        }

        throw new TimeoutException(
            $"Timeout ({timeout.TotalSeconds:0}s) acquiring lock '{lockPath}'. " +
            $"Probabile test bloccato o operazioni troppo lente. Ultimo errore: {last?.Message}");
    }

    public ValueTask DisposeAsync()
    {
        _stream.Dispose();
        return ValueTask.CompletedTask;
    }
}
