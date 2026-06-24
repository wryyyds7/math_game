using MathGame.Core.Data;

namespace MathGame.Core.Interfaces
{
    public interface ITurnManager
    {
        void StartGame(int playerCount);
        GamePhase CurrentPhase { get; }
        int CurrentTurnNumber { get; }
        void SubmitAction(int playerID, TurnActionType action, object actionData);
        void SkipCurrentPlayer();
        int GetCurrentPlayerID();
        float GetTurnTimeRemaining();
    }
}
