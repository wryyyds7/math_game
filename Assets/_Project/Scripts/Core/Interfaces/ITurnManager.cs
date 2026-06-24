using MathGame.Core.Data;

namespace MathGame.Core.Interfaces
{
    public interface ITurnManager
    {
        void StartGame(int playerCount, int aiCount = 0, Difficulty aiDifficulty = Difficulty.Normal);
        GamePhase CurrentPhase { get; }
        int CurrentTurnNumber { get; }
        void SubmitAction(int playerID, TurnActionType action, object actionData);
        void SkipCurrentPlayer();
        int GetCurrentPlayerID();
        float GetTurnTimeRemaining();
        bool IsCurrentPlayerAI { get; }
    }
}
