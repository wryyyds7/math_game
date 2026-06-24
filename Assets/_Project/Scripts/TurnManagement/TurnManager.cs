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
        private IAIModule aiModule;

        private TurnStateMachine stateMachine;
        private ActionResolver actionResolver;

        public int CurrentTurnNumber { get; private set; } = 0;
        public GamePhase CurrentPhase { get; private set; } = GamePhase.WaitingForPlayers;
        public bool IsCurrentPlayerAI
        {
            get
            {
                var p = playerManager?.GetCurrentPlayer();
                return p != null && p.IsAI;
            }
        }
        private float turnTimer;
        private bool aiActionReceived;

        private void Awake()
        {
            sceneManager = FindObjectOfType<SceneManager>();
            playerManager = FindObjectOfType<PlayerManager>();
            shootingSystem = FindObjectOfType<ShootingSystem>();
            mathParser = FindObjectOfType<MathParser>();
            aiModule = FindObjectOfType<AI.AIModule>();

            stateMachine = new TurnStateMachine();
            actionResolver = new ActionResolver
            {
                PlayerManager = playerManager,
                ShootingSystem = shootingSystem,
                MathParserInstance = mathParser,
                Config = config
            };
        }

        public void StartGame(int playerCount, int aiCount = 0, Difficulty aiDifficulty = Difficulty.Normal)
        {
            StartCoroutine(GameFlow(playerCount, aiCount, aiDifficulty));
        }

        private IEnumerator GameFlow(int playerCount, int aiCount, Difficulty aiDifficulty)
        {
            // 阶段1: 生成地图
            SetPhase(GamePhase.GeneratingMap);
            sceneManager.GenerateMap();
            yield return null;

            // 阶段2: 生成玩家
            SetPhase(GamePhase.WaitingForPlayers);
            for (int i = 0; i < playerCount; i++)
            {
                playerManager.SpawnPlayer($"玩家{i + 1}", isAI: false);
            }
            // 生成AI玩家
            if (aiCount > 0 && aiModule != null)
            {
                aiModule.Initialize(aiDifficulty);
                for (int i = 0; i < aiCount; i++)
                {
                    string aiName = aiDifficulty switch
                    {
                        Difficulty.Easy => $"简单AI-{i + 1}",
                        Difficulty.Normal => $"普通AI-{i + 1}",
                        Difficulty.Hard => $"困难AI-{i + 1}",
                        _ => $"AI-{i + 1}"
                    };
                    playerManager.SpawnPlayer(aiName, isAI: true);
                }
            }
            yield return new WaitForSeconds(1f);

            // 阶段3: 主循环
            while (playerManager.GetAlivePlayerCount() > 1)
            {
                CurrentTurnNumber++;
                var allPlayers = playerManager.GetAllPlayers();
                var obstacles = sceneManager.Obstacles;

                for (int i = 0; i < allPlayers.Count; i++)
                {
                    var player = allPlayers[i];
                    if (!player.IsAlive) continue;

                    SetPhase(GamePhase.PlayerTurn);
                    GameEvent.OnTurnStarted?.Invoke(CurrentTurnNumber);

                    if (player.IsAI && aiModule != null)
                    {
                        // === AI回合 ===
                        yield return HandleAITurn(player, allPlayers, obstacles);
                    }
                    else
                    {
                        // === 人类回合 ===
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

        /// <summary>处理AI玩家的回合</summary>
        private IEnumerator HandleAITurn(PlayerState aiPlayer,
                                          System.Collections.Generic.List<PlayerState> allPlayers,
                                          System.Collections.Generic.List<ObstacleData> obstacles)
        {
            aiActionReceived = false;

            aiModule.Think(aiPlayer, allPlayers, obstacles, (action, data) =>
            {
                SubmitAction(aiPlayer.PlayerID, action, data);
                aiActionReceived = true;
            });

            // 等待AI决策完成（最多等30秒，防止死循环）
            float waitTime = 0;
            while (!aiActionReceived && waitTime < 30f)
            {
                waitTime += Time.deltaTime;
                yield return null;
            }

            if (!aiActionReceived)
            {
                Debug.LogWarning($"[TurnManager] AI {aiPlayer.PlayerName} 超时，强制跳过");
                SkipCurrentPlayer();
            }
        }

        public void SubmitAction(int playerID, TurnActionType action, object actionData)
        {
            var player = playerManager.GetPlayer(playerID);
            if (player == null || !player.IsAlive) return;
            if (player.HasActedThisTurn) return;

            // 当前玩家检查（AI回合时放宽限制）
            if (!player.IsAI && player.PlayerID != GetCurrentPlayerID()) return;

            player.HasActedThisTurn = true;
            actionResolver.QueueAction(playerID, action, actionData);
            GameEvent.OnPlayerActionSelected?.Invoke(playerID, action);
            Debug.Log($"[TurnManager] {player.PlayerName} 提交动作: {action}");
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
