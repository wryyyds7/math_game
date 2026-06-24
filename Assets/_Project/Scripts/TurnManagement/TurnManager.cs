using System.Collections;
using UnityEngine;
using MathGame.Core.Data;
using MathGame.Core.Interfaces;
using MathGame.Player;

namespace MathGame.TurnManagement
{
    public class TurnManager : MonoBehaviour, ITurnManager
    {
        [SerializeField] private GameConfig config;
        [SerializeField] private float turnTimeLimit = 60f;

        private ISceneManager sceneManager;
        private IPlayerManager playerManager;
        private IShootingSystem shootingSystem;
        private IMathParser mathParser;

        private TurnStateMachine stateMachine;
        private ActionResolver actionResolver;

        public int CurrentTurnNumber { get; private set; } = 0;
        public GamePhase CurrentPhase { get; private set; } = GamePhase.WaitingForPlayers;
        private float turnTimer;

        private void Awake()
        {
            sceneManager = FindObjectOfType<SceneManager>();
            playerManager = FindObjectOfType<PlayerManager>();
            shootingSystem = FindObjectOfType<ShootingSystem>();
            mathParser = FindObjectOfType<MathParser>();

            stateMachine = new TurnStateMachine();
            actionResolver = new ActionResolver
            {
                PlayerManager = playerManager,
                ShootingSystem = shootingSystem,
                MathParserInstance = mathParser,
                Config = config
            };
        }

        public void StartGame(int playerCount)
        {
            StartCoroutine(GameFlow(playerCount));
        }

        private IEnumerator GameFlow(int playerCount)
        {
            // 阶段1: 生成地图
            SetPhase(GamePhase.GeneratingMap);
            sceneManager.GenerateMap();
            yield return null;

            // 阶段2: 生成玩家
            SetPhase(GamePhase.WaitingForPlayers);
            for (int i = 0; i < playerCount; i++)
            {
                playerManager.SpawnPlayer($"玩家{i + 1}");
            }
            yield return new WaitForSeconds(1f);

            // 阶段3: 主循环
            while (playerManager.GetAlivePlayerCount() > 1)
            {
                CurrentTurnNumber++;
                var allPlayers = playerManager.GetAllPlayers();

                for (int i = 0; i < allPlayers.Count; i++)
                {
                    var player = allPlayers[i];
                    if (!player.IsAlive) continue;

                    SetPhase(GamePhase.PlayerTurn);
                    GameEvent.OnTurnStarted?.Invoke(CurrentTurnNumber);

                    // 等待玩家提交动作
                    turnTimer = turnTimeLimit;
                    while (!player.HasActedThisTurn)
                    {
                        turnTimer -= Time.deltaTime;
                        if (turnTimeLimit > 0 && turnTimer <= 0)
                        {
                            SkipCurrentPlayer();
                            break;
                        }
                        yield return null;
                    }

                    // 结算动作
                    SetPhase(GamePhase.ResolvingAction);
                    yield return actionResolver.ResolveAction(player);

                    // 等待子弹飞行结束
                    while (shootingSystem.IsAnyBulletFlying)
                        yield return null;

                    player.HasActedThisTurn = false;
                    SetPhase(GamePhase.TurnEnd);
                    GameEvent.OnTurnEnded?.Invoke(CurrentTurnNumber);
                }
            }

            // 阶段4: 游戏结束
            SetPhase(GamePhase.GameOver);
            var winner = playerManager.GetAllPlayers().Find(p => p.IsAlive);
            GameEvent.OnGameOver?.Invoke(winner?.PlayerID ?? -1);
        }

        public void SubmitAction(int playerID, TurnActionType action, object actionData)
        {
            var player = playerManager.GetPlayer(playerID);
            if (player == null || player.PlayerID != GetCurrentPlayerID()) return;
            if (player.HasActedThisTurn) return;

            player.HasActedThisTurn = true;
            actionResolver.QueueAction(playerID, action, actionData);
            GameEvent.OnPlayerActionSelected?.Invoke(playerID, action);
            Debug.Log($"[TurnManager] 玩家{playerID} 提交动作: {action}");
        }

        public void SkipCurrentPlayer()
        {
            int id = GetCurrentPlayerID();
            var player = playerManager.GetPlayer(id);
            if (player != null) player.HasActedThisTurn = true;
            GameEvent.OnShowMessage?.Invoke($"{player?.PlayerName} 超时，跳过回合");
        }

        public int GetCurrentPlayerID()
        {
            return playerManager.GetCurrentPlayer()?.PlayerID ?? -1;
        }

        public float GetTurnTimeRemaining() => turnTimer;

        private void SetPhase(GamePhase phase)
        {
            CurrentPhase = phase;
            stateMachine.SetState(phase);
            GameEvent.OnPhaseChanged?.Invoke(phase);
        }
    }
}
