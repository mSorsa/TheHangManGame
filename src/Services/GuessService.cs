using TheHangManGame.Logging;
using TheHangManGame.Models;
using TheHangManGame.Services.Interfaces;

namespace TheHangManGame.Services;

public sealed class GuessService : IGuessService
{
    private readonly ILogger<GuessService> _logger;
    private readonly IPerformanceLogger _performance;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly IRandomWordService _randomWordService;

    private const int MaxGuesses = 6;
    private const string ViewBagCelebrationMessage = "Congratulations! You've found the word: ";
    private const string ViewBagDefeatMessage = "Alas, you failed to guess: ";
    private const string GoAgain = ". Let's try again!";

    public GuessService(ILogger<GuessService> logger, IHttpContextAccessor contextAccessor,
        IRandomWordService randomWordService, IPerformanceLogger performance)
    {
        _logger = logger;
        _contextAccessor = contextAccessor;
        _randomWordService = randomWordService;
        _performance = performance;
    }

    public async Task<ActiveWordModel> Handle()
    {
        await using (await _performance.TrackPerformanceAsync(nameof(Handle)))
        {
            var context = _contextAccessor.HttpContext ??
                          throw new ArgumentNullException(nameof(_contextAccessor.HttpContext),
                              "Could not resolve http context.");

            Enum.TryParse(context.Session.GetString("GameStatus") ?? "Ongoing", true,
                out GameStatus status);

            var gameEnded = false;
            var gameWin = status is GameStatus.Win;
            var endMessage = string.Empty;

            if (gameWin)
            {
                gameEnded = true;
                endMessage = ViewBagCelebrationMessage + AppendWordAndEndToMessage(context);

                await StartNewSession(context);
            }
            else if (status is GameStatus.Defeat)
            {
                gameEnded = true;
                endMessage = ViewBagDefeatMessage + AppendWordAndEndToMessage(context);

                await StartNewSession(context);
            }

            var word = GetSessionString(context, "RandomWord");
            if (string.IsNullOrWhiteSpace(word))
                word = await GetNewWord(context);

            var guessedLetters = GetSessionString(context, "CorrectGuesses");
            var incorrectGuesses = GetSessionString(context, "IncorrectGuesses");

            var displayWord = SetDisplayWordValues(word, guessedLetters);

            return new ActiveWordModel()
            {
                DisplayWord = displayWord,
                CurrentWord = word,
                IncorrectGuesses = incorrectGuesses,
                GuessedLetters = guessedLetters,
                GameEnd = gameEnded,
                GameWin = gameWin,
                EndMessage = endMessage
            };
        }
    }

    private static string AppendWordAndEndToMessage(HttpContext context)
    {
        return GetSessionString(context, "RandomWord") + GoAgain;
    }

    private async Task StartNewSession(HttpContext context)
    {
        await using (await _performance.TrackPerformanceAsync(nameof(StartNewSession)))
        {
            // Reset the game state for a new session
            context.Session.Clear();

            // Start a new game immediately by setting a new random word
            await GetNewWord(context);
        }
    }

    private async Task<string> GetNewWord(HttpContext context)
    {
        string word;
        try
        {
            word = await _randomWordService.GetRandomWord();
        }
        catch (Exception e)
        {
            _logger.LogWarning(new EventId(1, "Exception retrieving new word"), e,
                "Failed to retrieve word from Random Word API: {message}", e.Message);
            throw;
        }

        SetSessionString(context, "RandomWord", word);

        return word;
    }

    private static void SetSessionString(HttpContext ctx, string key, string value)
    {
        ctx.Session.SetString(key, value);
    }

    private static string GetSessionString(HttpContext ctx, string key)
    {
        return ctx.Session.GetString(key)?.ToUpper() ?? string.Empty;
    }

    private static string SetDisplayWordValues(string word, string guessedLetters)
    {
        return string.Concat(word
            .Select(c => guessedLetters.ToUpper().Contains(c) 
                ? c + " " 
                : "_ "));
    }

    public void HandleGuess(char guess)
    {
        using (_performance.TrackPerformance(nameof(HandleGuess)))
        {
            var context = _contextAccessor.HttpContext ??
                          throw new ArgumentNullException(nameof(_contextAccessor.HttpContext),
                              "Could not resolve http context.");

            VerifyGuess(context, guess);

            var word = GetSessionString(context, "RandomWord");
            var correctGuesses = GetSessionString(context, "CorrectGuesses");
            var incorrectGuesses = GetSessionString(context, "IncorrectGuesses");

            if (word.All(letter => correctGuesses.Contains(letter)))
            {
                _logger.LogDebug(
                    "Game Won; word: {activeWord}. Correct guesses: {guesses}. Incorrect guessed letters: {incorrectGuesses}.",
                    word, correctGuesses, incorrectGuesses);
                SetSessionString(context, "GameStatus", GameStatus.Win.ToString());
            }
            else if (incorrectGuesses?.Length > MaxGuesses)
            {
                _logger.LogInformation(
                    "Game Failed; word: {activeWord}. Correct guesses: {guesses}. Guessed letters: {incorrectGuesses}.",
                    word, correctGuesses, incorrectGuesses);
                SetSessionString(context, "GameStatus", GameStatus.Defeat.ToString());
            }
            else
            {
                _logger.LogDebug(
                    "Game ongoing; word: {activeWord}. Correct guesses: {guesses}. Guessed letters: {incorrectGuesses}. " +
                    "Number of chances left: {noTriesLeft}", word, correctGuesses, incorrectGuesses,
                    MaxGuesses - incorrectGuesses?.Length);
                SetSessionString(context, "GameStatus", GameStatus.Ongoing.ToString());
            }
        }
    }

    private void VerifyGuess(HttpContext context, char guess)
    {
        using (_performance.TrackPerformance(nameof(VerifyGuess)))
        {
            var word = GetSessionString(context, "RandomWord");

            if (word is "")
            {
                throw new TimeoutException("No word found in session. Starting new game.");
            }

            var correctGuesses = GetSessionString(context, "CorrectGuesses");
            var incorrectGuesses = GetSessionString(context, "IncorrectGuesses").ToList();

            // Check if the letter has already been guessed
            if (correctGuesses.Contains(guess) || incorrectGuesses.Contains(guess))
            {
                throw new ArgumentException("You already guessed that letter.");
            }

            // Check if the guessed letter is in the word
            if (word.Contains(guess))
            {
                correctGuesses += guess;
            }
            else
            {
                incorrectGuesses.Add(guess);
            }

            SetSessionString(context, "CorrectGuesses", correctGuesses);
            SetSessionString(context, "IncorrectGuesses", new string(incorrectGuesses.ToArray()));
        }
    }
}