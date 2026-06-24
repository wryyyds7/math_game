namespace MathGame.Core.Data
{
    public enum GamePhase
    {
        WaitingForPlayers = 0,
        GeneratingMap = 1,
        PlayerTurn = 2,
        ResolvingAction = 3,
        TurnEnd = 4,
        GameOver = 5
    }
}
