using TheHangManGame.Models;

namespace TheHangManGame.Services.Interfaces;

public interface IGuessService
{
    Task<ActiveWordModel> Handle();
    void HandleGuess(char guess);
}