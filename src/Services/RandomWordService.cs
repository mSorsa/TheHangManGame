using Newtonsoft.Json;
using TheHangManGame.Logging;
using TheHangManGame.Services.Interfaces;

namespace TheHangManGame.Services;

public sealed class RandomWordService : IRandomWordService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<RandomWordService> _logger;
    private readonly IPerformanceLogger _performance;

    // URL of the Random Word API
    private const string Url = "https://random-word-api.herokuapp.com/word";


    public RandomWordService(HttpClient httpClient, ILogger<RandomWordService> logger, IPerformanceLogger performance)
    {
        _httpClient = httpClient;
        _logger = logger;
        _performance = performance;
    }

    public async Task<string> GetRandomWord()
    {
        await using (await _performance.TrackPerformanceAsync(nameof(GetRandomWord)))
        {
            try
            {
                var jsonResponse = await _httpClient.GetStringAsync(Url);

                var words = JsonConvert.DeserializeObject<List<string>>(jsonResponse);

                return words?.FirstOrDefault() ??
                       throw new ArgumentNullException(nameof(words), "Random-word-api returned invalid response.");
            }
            catch (HttpRequestException httpException)
            {
                _logger.LogError(httpException, "Failure to retrieve random word. Exception: {message}",
                    httpException.Message);
                throw;
            }
            catch (ArgumentNullException nullException)
            {
                _logger.LogError(nullException, "Word retrieved from Random Word Api is null. Exception: {message}",
                    nullException.Message);
                throw;
            }
        }
    }
}