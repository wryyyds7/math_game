using UnityEngine;
using MathGame.Core.Data;
using MathGame.Core.Interfaces;
using MathGame.Player;
using MathGame.SceneManagement;
using MathGame.Shooting;
using MathGame.TurnManagement;
using MathGame.UI;
using MathGame.MathParser;

namespace MathGame.Core
{
    /// <summary>
    /// GameBootstrap — 游戏入口，挂在场景中的空GameObject上
    /// 负责初始化依赖注入和启动游戏
    /// V2: 支持人机模式 + 模式选择
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        [Header("全局配置")]
        [SerializeField] private GameConfig gameConfig;

        [Header("模块引用（自动查找）")]
        [SerializeField] private SceneManager sceneManager;
        [SerializeField] private PlayerManager playerManager;
        [SerializeField] private MathParser mathParser;
        [SerializeField] private ShootingSystem shootingSystem;
        [SerializeField] private TurnManager turnManager;
        [SerializeField] private UIManager uiManager;
        [SerializeField] private AI.AIModule aiModule;

        private ITurnManager ITurnManager => turnManager;
        private IUIManager IUIManager => uiManager;
        private IPlayerManager IPlayerManager => playerManager;

        [Header("测试模式")]
        [SerializeField] private bool autoStart = false;
        [SerializeField] private int autoHumanCount = 1;
        [SerializeField] private int autoAICount = 1;
        [SerializeField] private Difficulty autoAIDifficulty = Difficulty.Normal;

        private void Awake()
        {
            // 自动查找模块引用
            if (sceneManager == null) sceneManager = FindObjectOfType<SceneManager>();
            if (playerManager == null) playerManager = FindObjectOfType<PlayerManager>();
            if (mathParser == null) mathParser = FindObjectOfType<MathParser>();
            if (shootingSystem == null) shootingSystem = FindObjectOfType<ShootingSystem>();
            if (turnManager == null) turnManager = FindObjectOfType<TurnManager>();
            if (uiManager == null) uiManager = FindObjectOfType<UIManager>();
            if (aiModule == null) aiModule = FindObjectOfType<AI.AIModule>();
        }

        private void Start()
        {
            // 监听UI事件
            if (uiManager != null)
            {
                // 新版：带模式的开始游戏
                uiManager.OnGameStartRequested += HandleGameStartWithMode;
                // 旧版兼容
                uiManager.OnStartGameClicked += () => HandleGameStartWithMode(GameMode.PvP, Difficulty.Normal);
                uiManager.OnShootSubmitted += HandleShoot;
                uiManager.OnBlinkSubmitted += HandleBlink;
                uiManager.OnRotateSubmitted += HandleRotate;
            }

            // 监听游戏流程事件
            GameEvent.OnTurnStarted += HandleTurnStarted;
            GameEvent.OnPhaseChanged += HandlePhaseChanged;
            GameEvent.OnPlayerEliminated += HandlePlayerEliminated;

            // 自动开始测试
            if (autoStart)
            {
                HandleGameStartWithMode(autoAICount > 0 ? GameMode.PvAI : GameMode.PvP, autoAIDifficulty);
            }
        }

        private void OnDestroy()
        {
            if (uiManager != null)
            {
                uiManager.OnGameStartRequested -= HandleGameStartWithMode;
                uiManager.OnStartGameClicked -= () => HandleGameStartWithMode(GameMode.PvP, Difficulty.Normal);
                uiManager.OnShootSubmitted -= HandleShoot;
                uiManager.OnBlinkSubmitted -= HandleBlink;
                uiManager.OnRotateSubmitted -= HandleRotate;
            }

            GameEvent.OnTurnStarted -= HandleTurnStarted;
            GameEvent.OnPhaseChanged -= HandlePhaseChanged;
            GameEvent.OnPlayerEliminated -= HandlePlayerEliminated;
            GameEvent.ClearAllEvents();
        }

        // ===== 事件处理 =====

        /// <summary>新版开始游戏：支持模式选择</summary>
        public void HandleGameStartWithMode(GameMode mode, Difficulty difficulty)
        {
            Debug.Log($"[GameBootstrap] 开始游戏 — 模式:{mode}, 难度:{difficulty}");
            uiManager.ShowGameHUD();

            int humanCount = mode == GameMode.PvAI ? 1 : 2;
            int aiCount = mode == GameMode.PvAI ? 1 : 0;

            turnManager.StartGame(humanCount, aiCount, difficulty);
        }

        public void HandleStartGame()
        {
            // 兼容旧版：默认PvP
            Debug.Log("[GameBootstrap] 开始游戏 (旧版兼容)");
            uiManager.ShowGameHUD();
            turnManager.StartGame(2, 0);
        }

        private void HandleShoot(string expression)
        {
            int playerID = turnManager.GetCurrentPlayerID();
            var player = playerManager.GetPlayer(playerID);
            if (player == null || player.IsAI) return; // AI不通过UI提交

            if (!string.IsNullOrEmpty(expression))
            {
                var curve = mathParser.ParseAndGenerate(
                    expression, player.Position, player.Rotation, gameConfig.MaxCurveLength);
                uiManager.UpdateCurvePreview(curve);
            }
            turnManager.SubmitAction(playerID, TurnActionType.Shoot, expression);
        }

        private void HandleBlink(Vector2 target)
        {
            int playerID = turnManager.GetCurrentPlayerID();
            var player = playerManager.GetPlayer(playerID);
            if (player == null || player.IsAI) return;

            // 默认闪现到前方半程
            Vector2 blinkTarget = target;
            if (target == Vector2.zero)
            {
                blinkTarget = player.Position +
                    player.Forward * player.BlinkRange(gameConfig.MapWidth, gameConfig.MapHeight) * 0.5f;
            }
            turnManager.SubmitAction(playerID, TurnActionType.Blink, blinkTarget);
        }

        private void HandleRotate()
        {
            int playerID = turnManager.GetCurrentPlayerID();
            var player = playerManager.GetPlayer(playerID);
            if (player == null || player.IsAI) return;

            turnManager.SubmitAction(playerID, TurnActionType.Rotate180, null);
        }

        private void HandleTurnStarted(int turnNumber)
        {
            Debug.Log($"[GameBootstrap] ===== 回合 {turnNumber} 开始 =====");
            var currentPlayer = playerManager.GetCurrentPlayer();
            if (currentPlayer != null)
            {
                uiManager.RefreshHUD(currentPlayer, turnNumber);
            }
        }

        private void HandlePhaseChanged(GamePhase phase)
        {
            Debug.Log($"[GameBootstrap] 阶段变更: {phase}");
            switch (phase)
            {
                case GamePhase.PlayerTurn:
                    var current = playerManager.GetCurrentPlayer();
                    if (current != null && !current.IsAI)
                        uiManager.ShowActionPanel();
                    break;
                case GamePhase.TurnEnd:
                    uiManager.HideActionPanel();
                    break;
                case GamePhase.GameOver:
                    break;
            }
        }

        private void HandlePlayerEliminated(int playerID)
        {
            var player = playerManager.GetPlayer(playerID);
            if (player != null)
                Debug.Log($"[GameBootstrap] {player.PlayerName} 被淘汰！");
        }
    }
}
