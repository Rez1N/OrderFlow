using System.Collections.Concurrent;
using System.Text.Json;
using OrderFlow.Console.Models;
using OrderFlow.Console.Services;

namespace OrderFlow.Console.Watchers;

public sealed class InboxWatcher : IDisposable
{
    private readonly string _inboxPath;
    private readonly OrderPipeline _pipeline;
    private readonly FileSystemWatcher _watcher;
    private readonly SemaphoreSlim _semaphore;
    private readonly ConcurrentDictionary<string, byte> _inProgress = new(StringComparer.OrdinalIgnoreCase);
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };
    private bool _disposed;

    public InboxWatcher(string inboxPath, OrderPipeline pipeline, int maxConcurrentFiles = 2)
    {
        _inboxPath = inboxPath;
        _pipeline = pipeline;
        _semaphore = new SemaphoreSlim(maxConcurrentFiles, maxConcurrentFiles);

        Directory.CreateDirectory(_inboxPath);
        Directory.CreateDirectory(Path.Combine(_inboxPath, "processed"));
        Directory.CreateDirectory(Path.Combine(_inboxPath, "failed"));

        _watcher = new FileSystemWatcher(_inboxPath, "*.json")
        {
            IncludeSubdirectories = false,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime | NotifyFilters.Size
        };
        _watcher.Created += OnCreated;
    }

    public void Start()
    {
        EnsureNotDisposed();
        _watcher.EnableRaisingEvents = true;
        System.Console.WriteLine($"[WATCHER] Monitoring folder: {_inboxPath}");
    }

    private void OnCreated(object sender, FileSystemEventArgs e)
    {
        if (!_inProgress.TryAdd(e.FullPath, 1))
            return;

        _ = Task.Run(async () =>
        {
            try
            {
                await ProcessFileAsync(e.FullPath);
            }
            finally
            {
                _inProgress.TryRemove(e.FullPath, out _);
            }
        });
    }

    private async Task ProcessFileAsync(string path)
    {
        await _semaphore.WaitAsync();
        try
        {
            System.Console.WriteLine($"[WATCHER] File detected: {Path.GetFileName(path)}");
            var orders = await ReadOrdersWithRetryAsync(path);

            foreach (var order in orders)
                await _pipeline.ProcessOrderAsync(order);

            var processedPath = GetTargetPath("processed", path);
            File.Move(path, processedPath, overwrite: true);
            System.Console.WriteLine($"[WATCHER] Imported {orders.Count} orders -> {processedPath}");
        }
        catch (Exception ex)
        {
            var failedPath = GetTargetPath("failed", path);
            try
            {
                if (File.Exists(path))
                    File.Move(path, failedPath, overwrite: true);
                await File.WriteAllTextAsync(failedPath + ".error.txt", ex.ToString());
            }
            catch
            {
                // No-op: keep watcher alive on secondary failures.
            }

            System.Console.WriteLine($"[WATCHER] Failed importing {Path.GetFileName(path)}: {ex.Message}");
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task<List<Order>> ReadOrdersWithRetryAsync(string path)
    {
        const int maxAttempts = 5;
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                await Task.Delay(250);
                await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                var orders = await JsonSerializer.DeserializeAsync<List<Order>>(stream, _jsonOptions);
                if (orders is { Count: > 0 })
                    return orders;
            }
            catch (IOException) when (attempt < maxAttempts)
            {
                await Task.Delay(200);
            }
            catch (JsonException) when (attempt < maxAttempts)
            {
                await Task.Delay(200);
            }
        }

        await using var fallbackStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        var singleOrder = await JsonSerializer.DeserializeAsync<Order>(fallbackStream, _jsonOptions);
        return singleOrder is null ? new List<Order>() : new List<Order> { singleOrder };
    }

    private string GetTargetPath(string bucket, string sourcePath)
        => Path.Combine(_inboxPath, bucket, Path.GetFileName(sourcePath));

    private void EnsureNotDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(InboxWatcher));
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _watcher.EnableRaisingEvents = false;
        _watcher.Created -= OnCreated;
        _watcher.Dispose();
        _semaphore.Dispose();
        _disposed = true;
    }
}
