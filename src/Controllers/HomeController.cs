using Microsoft.AspNetCore.Mvc;
using TheHangManGame.Logging;
using TheHangManGame.Models;
using TheHangManGame.Services.Interfaces;

namespace TheHangManGame.Controllers;

public sealed class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IGuessService _guessService;
    private readonly IPerformanceLogger _performance;

    public HomeController(ILogger<HomeController> logger, IGuessService guessService, IPerformanceLogger performance)
    {
        _logger = logger;
        _guessService = guessService;
        _performance = performance;
    }

    public async Task<IActionResult> Index()
    {
        await using (await _performance.TrackPerformanceAsync(nameof(Index)))
        {
            ActiveWordModel activeWord;
            try
            {
                activeWord = await _guessService.Handle();
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Exception Handling: {message}", e.Message);
                TempData["Error"] = e.Message;

                return View();
            }

            ViewBag.CurrentState = activeWord.DisplayWord.Trim();
            ViewBag.IncorrectGuesses = activeWord.IncorrectGuesses;
            ViewBag.Error = TempData["Error"];
            ViewBag.GameEnd = activeWord.GameEnd;

            if (!activeWord.GameEnd)
                return View();

            if (!activeWord.GameEnd)
                return View();

            ViewBag.CelebrationMessage = activeWord.EndMessage;
            ViewBag.GameWin = activeWord.GameWin;

            return View();
        }
    }

    [HttpPost]
    public async Task<IActionResult> MakeGuess(char guess)
    {
        await using (await _performance.TrackPerformanceAsync(nameof(MakeGuess)))
        {
            if (!char.IsLetter(guess))
            {
                // If not, store an error message and redirect back to the Index action
                TempData["Error"] = "Please enter a letter from A-Z.";
                return RedirectToAction("Index");
            }

            try
            {
                _guessService.HandleGuess(char.ToUpper(guess));
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Exception when guessing: {message}", e.Message);
                TempData["Error"] = e.Message;
            }

            // Redirect to the Index action to display the updated game state
            return RedirectToAction("Index");
        }
    }
}