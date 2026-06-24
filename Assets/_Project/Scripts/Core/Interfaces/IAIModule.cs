using System;
using System.Collections.Generic;
using MathGame.Core.Data;

namespace MathGame.Core.Interfaces
{
    public interface IAIModule
    {
        void Initialize(Difficulty difficulty);
        void Think(PlayerState aiState, List<PlayerState> allPlayers,
                   List<ObstacleData> obstacles, Action<TurnActionType, object> onActionReady);
        Difficulty CurrentDifficulty { get; }
    }
}
