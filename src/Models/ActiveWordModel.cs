namespace TheHangManGame.Models;

public class ActiveWordModel
{
    public bool GameEnd { get; set; } = false;
    public bool GameWin { get; set; } = false;
    public string EndMessage { get; set; } = "";
    public string DisplayWord { get; set; } = "";
    public string CurrentWord { get; set; } = "";
    public string IncorrectGuesses { get; set; } = "";
    public string GuessedLetters { get; set; } = "";
}