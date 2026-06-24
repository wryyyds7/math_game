namespace MathGame.Core.Interfaces
{
    public interface ILocalMultiplayerManager
    {
        void SetupPlayers(int count, string[] names);
        string[] GetPlayerNames();
        void StartLocalGame();
    }
}
