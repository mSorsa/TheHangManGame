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

    /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources asynchronously.</summary>
    /// <returns>A task that represents the asynchronous dispose operation.</returns>
    public ValueTask DisposeAsync()
    {
        _timer.Stop();
        _logger.LogDebug(new EventId(0, "Time-Taken"), "Time-Taken ({ms}ms) for: {method}", _timer.ElapsedMilliseconds,
            _message);

        return ValueTask.CompletedTask;
    }

    /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
    public void Dispose()
    {
        _timer.Stop();
        _logger.LogDebug(new EventId(0, "Time-Taken"), "Time-Taken ({ms}ms) for: {method}", _timer.ElapsedMilliseconds,
            _message);
    }
}