namespace TheHangManGame.Logging;

public interface IPerformanceLogger
{
    Task<IAsyncDisposable> TrackPerformanceAsync(string methodName);
    IDisposable TrackPerformance(string methodName);
}