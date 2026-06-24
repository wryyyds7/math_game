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

        private ITurnManager ITurnManager => turnManager;
        private IUIManager IUIManager => uiManager;
        private IPlayerManager IPlayerManager => playerManager;

        [Header("测试模式")]
        [SerializeField] private bool autoStart = false;
        [SerializeField] private int testPlayerCount = 2;

        private void Awake()
        {
            // 自动查找模块引用
            if (sceneManager == null) sceneManager = FindObjectOfType<SceneManager>();
            if (playerManager == null) playerManager = FindObjectOfType<PlayerManager>();
            if (mathParser == null) mathParser = FindObjectOfType<MathParser>();
            if (shootingSystem == null) shootingSystem = FindObjectOfType<ShootingSystem>();
            if (turnManager == null) turnManager = FindObjectOfType<TurnManager>();
            if (uiManager == null) uiManager = FindObjectOfType<UIManager>();
        }

        private void Start()
        {
            // 监听UI事件
            if (uiManager != null)
            {
                uiManager.OnStartGameClicked += HandleStartGame;
                uiManager.OnShootSubmitted += HandleShoot;
                uiManager.OnBlinkSubmitted += HandleBlink;
                uiManager.OnRotateSubmitted += HandleRotate;
            }

            // 监听游戏流程事件
            GameEvent.OnTurnStarted += HandleTurnStarted;
            GameEvent.OnPhaseChanged += HandlePhaseChanged;
            GameEvent.OnPlayerEliminated += HandlePlayerEliminated;
        }

        private void OnDestroy()
        {
            if (uiManager != null)
            {
                uiManager.OnStartGameClicked -= HandleStartGame;
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

        public void HandleStartGame()
        {
            Debug.Log("[GameBootstrap] 开始游戏");
            uiManager.ShowGameHUD();
            turnManager.StartGame(testPlayerCount);
        }

        private void HandleShoot(string expression)
        {
            int playerID = turnManager.GetCurrentPlayerID();
            var player = playerManager.GetPlayer(playerID);

            if (player != null)
            {
                // 先预览曲线
                var curve = mathParser.ParseAndGenerate(
                    expression, player.Position, player.Rotation, gameConfig.MaxCurveLength);
                uiManager.UpdateCurvePreview(curve);

                // 提交射击（确认后再调用）
                // 这里简化：直接提交
            }
            turnManager.SubmitAction(playerID, TurnActionType.Shoot, expression);
        }

        private void HandleBlink(Vector2 target)
        {
            int playerID = turnManager.GetCurrentPlayerID();
            // target 由地图点击系统提供，这里简化
            var player = playerManager.GetPlayer(playerID);
            if (player != null)
            {
                // 默认闪现到前方半程
                Vector2 blinkTarget = player.Position +
                    player.Forward * player.BlinkRange(gameConfig.MapWidth, gameConfig.MapHeight) * 0.5f;
                turnManager.SubmitAction(playerID, TurnActionType.Blink, blinkTarget);
            }
        }

        private void HandleRotate()
        {
            int playerID = turnManager.GetCurrentPlayerID();
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
                    uiManager.ShowActionPanel();
                    break;
                case GamePhase.TurnEnd:
                    uiManager.HideActionPanel();
                    break;
                case GamePhase.GameOver:
                    // GameOver由UIManager监听GameEvent处理
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
