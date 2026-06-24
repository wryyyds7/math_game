using System.Collections.Generic;
using UnityEngine;
using MathGame.Core.Data;
using MathGame.Core.Interfaces;

namespace MathGame.Player
{
    public class PlayerManager : MonoBehaviour, IPlayerManager
    {
        [SerializeField] private GameConfig config;

        private ISceneManager sceneManager;
        private List<PlayerState> players = new();
        private int nextPlayerID = 0;
        private int currentPlayerIndex = 0;

        [SerializeField] private GameObject playerPrefab;
        private List<GameObject> playerObjects = new();

        public List<PlayerState> GetAllPlayers() => players;
        public PlayerState GetPlayer(int id) => players.Find(p => p.PlayerID == id);

        public PlayerState GetCurrentPlayer()
        {
            if (players.Count == 0) return null;
            return players[currentPlayerIndex];
        }

        public int GetAlivePlayerCount() =>
            players.FindAll(p => p.IsAlive).Count;

        public bool IsPlayerAlive(int id) => GetPlayer(id)?.IsAlive ?? false;

        private void Awake()
        {
            sceneManager = FindObjectOfType<SceneManager>();
        }

        public PlayerState SpawnPlayer(string name, bool isAI = false)
        {
            Vector2 pos = sceneManager.GetValidSpawnPoint();
            float rot = Random.Range(0f, 360f);
            var state = new PlayerState
            {
                PlayerID = nextPlayerID++,
                PlayerName = name,
                Position = pos,
                Rotation = rot,
                IsAI = isAI,
                BlinkCharges = config.MaxBlinkCharges,
                MaxBlinkCharges = config.MaxBlinkCharges
            };
            players.Add(state);

            // 实例化GameObject
            if (playerPrefab != null)
            {
                var obj = Instantiate(playerPrefab, pos, Quaternion.identity);
                obj.name = $"Player_{state.PlayerID}_{name}";
                var controller = obj.GetComponent<PlayerController>();
                controller?.Initialize(state.PlayerID, this);
                playerObjects.Add(obj);
            }

            GameEvent.OnPlayerSpawned?.Invoke(state);
            Debug.Log($"[PlayerManager] 生成玩家: {name} at {pos}");
            return state;
        }

        public bool TryBlink(int playerID, Vector2 target)
        {
            var player = GetPlayer(playerID);
            if (player == null || !player.IsAlive || player.BlinkCharges <= 0)
                return false;

            float blinkRange = player.BlinkRange(config.MapWidth, config.MapHeight);
            if (Vector2.Distance(player.Position, target) > blinkRange)
                return false;
            if (!sceneManager.IsInsideMap(target))
                return false;
            // 不受障碍物阻挡

            player.Position = target;
            player.BlinkCharges--;
            GameEvent.OnPlayerBlinked?.Invoke(playerID, target);
            GameEvent.OnShowMessage?.Invoke($"{player.PlayerName} 闪现！剩余 {player.BlinkCharges} 次");
            return true;
        }

        public void RotatePlayer(int playerID)
        {
            var player = GetPlayer(playerID);
            if (player == null) return;
            player.Rotation = (player.Rotation + 180f) % 360f;
            Debug.Log($"[PlayerManager] {player.PlayerName} 旋转180度，新朝向: {player.Rotation}");
        }

        public (bool leveledUp, int newLevel) AddExp(int playerID, int exp)
        {
            var player = GetPlayer(playerID);
            if (player == null) return (false, player?.Level ?? 1);

            var result = LevelConfig.AddExp(player.Level, player.CurrentExp, exp);
            player.CurrentExp = result.remainingExp;
            player.Level = result.newLevel;

            if (result.leveledUp)
                GameEvent.OnPlayerLevelUp?.Invoke(playerID, result.newLevel);
            GameEvent.OnPlayerExpChanged?.Invoke(playerID, player.CurrentExp);

            return (result.leveledUp, result.newLevel);
        }

        public void EliminatePlayer(int playerID)
        {
            var player = GetPlayer(playerID);
            if (player == null || !player.IsAlive) return;

            player.IsAlive = false;
            GameEvent.OnPlayerEliminated?.Invoke(playerID);
            GameEvent.OnShowMessage?.Invoke($"{player.PlayerName} 被淘汰！");
        }

        public void AdvanceToNextPlayer()
        {
            int start = currentPlayerIndex;
            do
            {
                currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
            }
            while (!players[currentPlayerIndex].IsAlive && currentPlayerIndex != start);
        }
    }
}
