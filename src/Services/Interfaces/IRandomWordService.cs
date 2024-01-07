namespace TheHangManGame.Services.Interfaces;

public interface IRandomWordService
{
    Task<string> GetRandomWord();
}