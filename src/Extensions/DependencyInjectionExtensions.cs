using TheHangManGame.Logging;
using TheHangManGame.Services;
using TheHangManGame.Services.Interfaces;

namespace TheHangManGame.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddDependencyInjection(this IServiceCollection services)
    {
        services.AddHttpClient();
        services.AddHttpContextAccessor();
        services.AddScoped<IRandomWordService, RandomWordService>();
        services.AddScoped<IGuessService, GuessService>();
        services.AddTransient<IPerformanceLogger, PerformanceLogger>();

        return services;
    }

    public static IServiceCollection AddSessionWithCache(this IServiceCollection services)
    {
        services.AddDistributedMemoryCache();
        services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromSeconds(20);  // User has 20 seconds to make a guess, resets after each guess.
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });
        
        return services;
    }
}
