using System;
using UnityEngine;

namespace MathGame.Core.Data
{
    public static class GameEvent
    {
        // === 游戏流程 ===
        public static event Action<GamePhase> OnPhaseChanged;
        public static event Action<int> OnTurnStarted;
        public static event Action<int> OnTurnEnded;
        public static event Action<int> OnGameOver;

        // === 玩家事件 ===
        public static event Action<PlayerState> OnPlayerSpawned;
        public static event Action<int> OnPlayerEliminated;
        public static event Action<int, int> OnPlayerLevelUp;
        public static event Action<int, int> OnPlayerExpChanged;

        // === 回合动作 ===
        public static event Action<int, TurnActionType> OnPlayerActionSelected;
        public static event Action<BulletInfo> OnBulletFired;
        public static event Action<BulletInfo> OnBulletHitObstacle;
        public static event Action<int, int> OnBulletHitPlayer;
        public static event Action<int, Vector2> OnPlayerBlinked;

        // === 道具 ===
        public static event Action<int, StarData> OnStarCollected;

        // === UI事件 ===
        public static event Action<string> OnShowMessage;
        public static event Action<BulletInfo> OnRequestBulletPreview;

        public static void ClearAllEvents()
        {
            OnPhaseChanged = null;
            OnTurnStarted = null;
            OnTurnEnded = null;
            OnGameOver = null;
            OnPlayerSpawned = null;
            OnPlayerEliminated = null;
            OnPlayerLevelUp = null;
            OnPlayerExpChanged = null;
            OnPlayerActionSelected = null;
            OnBulletFired = null;
            OnBulletHitObstacle = null;
            OnBulletHitPlayer = null;
            OnPlayerBlinked = null;
            OnStarCollected = null;
            OnShowMessage = null;
            OnRequestBulletPreview = null;
        }
    }
}
