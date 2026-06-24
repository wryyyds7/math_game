using System.Collections.Generic;
using MathGame.Core.Data;
using UnityEngine;

namespace MathGame.TurnManagement
{
    public class TurnStateMachine
    {
        public GamePhase CurrentState { get; private set; } = GamePhase.WaitingForPlayers;

        private static readonly HashSet<(GamePhase, GamePhase)> ValidTransitions = new()
        {
            (GamePhase.WaitingForPlayers, GamePhase.GeneratingMap),
            (GamePhase.GeneratingMap,     GamePhase.WaitingForPlayers),
            (GamePhase.WaitingForPlayers, GamePhase.PlayerTurn),
            (GamePhase.PlayerTurn,        GamePhase.ResolvingAction),
            (GamePhase.ResolvingAction,   GamePhase.TurnEnd),
            (GamePhase.TurnEnd,           GamePhase.PlayerTurn),
            (GamePhase.TurnEnd,           GamePhase.GameOver),
        };

        public bool CanTransition(GamePhase from, GamePhase to)
            => ValidTransitions.Contains((from, to));

        public void SetState(GamePhase newState)
        {
            if (CanTransition(CurrentState, newState))
            {
                CurrentState = newState;
            }
            else
            {
                Debug.LogWarning($"[TurnStateMachine] 非法状态转换: {CurrentState} -> {newState}");
            }
        }
    }
}
