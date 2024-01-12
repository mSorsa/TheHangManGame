using System.Diagnostics;

namespace TheHangManGame.Logging;

public sealed class PerformanceLogger : IPerformanceLogger
{
    private readonly ILogger<PerformanceLogger> _logger;

    public PerformanceLogger(ILogger<PerformanceLogger> logger)
    {
        _logger = logger;
    }

    public async Task<IAsyncDisposable> TrackPerformanceAsync(string methodName)
    {
        return await Task.FromResult(new PerformanceTracker(_logger, methodName));
    }

    public IDisposable TrackPerformance(string methodName)
    {
        return new PerformanceTracker(_logger, methodName);
    }
}