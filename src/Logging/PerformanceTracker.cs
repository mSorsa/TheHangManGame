using System.Diagnostics;

namespace TheHangManGame.Logging;

public sealed class PerformanceTracker : IAsyncDisposable, IDisposable
{
    private readonly ILogger _logger;
    private readonly Stopwatch _timer;
    private readonly string _message;


    public PerformanceTracker(ILogger logger, string message)
    {
        _logger = logger;
        _timer = Stopwatch.StartNew();
        _message = message;
    }

    public ValueTask DisposeAsync()
    {
        _timer.Stop();
        _logger.LogDebug(new EventId(0, "Time-Taken"), "Time-Taken ({ms}ms) for: {method}", _timer.ElapsedMilliseconds,
            _message);

        return ValueTask.CompletedTask;
    }

    public void Dispose()
    {
        _timer.Stop();
        _logger.LogDebug(new EventId(0, "Time-Taken"), "Time-Taken ({ms}ms) for: {method}", _timer.ElapsedMilliseconds,
            _message);
    }
}