using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MathGame.Core.Data;
using MathGame.Core.Interfaces;
using MathGame.Player;

namespace MathGame.UI
{
    /// <summary>
    /// 游戏模式枚举
    /// </summary>
    public enum GameMode
    {
        PvP,      // 双人对战
        PvAI      // 人机对战
    }

    public class UIManager : MonoBehaviour, IUIManager
    {
        [Header("面板")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject gameHUDPanel;
        [SerializeField] private MathInputPanel mathInputPanel;
        [SerializeField] private TurnActionSelector actionSelector;
        [SerializeField] private HUDController hudController;
        [SerializeField] private GameOverPanel gameOverPanel;
        [SerializeField] private SettingsPanel settingsPanel;

        [Header("主菜单 - 模式选择")]
        [SerializeField] private Button pvpButton;
        [SerializeField] private Button pvaiButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button exitButton;

        [Header("主菜单 - 难度选择 (人机模式时显示)")]
        [SerializeField] private GameObject difficultySelector;
        [SerializeField] private Button easyButton;
        [SerializeField] private Button normalButton;
        [SerializeField] private Button hardButton;

        [Header("消息提示")]
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private float messageFadeTime = 2f;

        [Header("曲线预览")]
        [SerializeField] private LineRenderer curvePreviewLine;

        // 事件
        public event Action OnStartGameClicked;  // 旧版兼容
        public event Action<GameMode, Difficulty> OnGameStartRequested;  // 新版：模式+难度
        public event Action<string> OnShootSubmitted;
        public event Action<Vector2> OnBlinkSubmitted;
        public event Action OnRotateSubmitted;

        private IMathParser mathParser;
        private ITurnManager turnManager;
        private IPlayerManager playerManager;
        private Coroutine messageCoroutine;

        private GameMode selectedMode = GameMode.PvP;
        private Difficulty selectedDifficulty = Difficulty.Normal;

        private void Awake()
        {
            mathParser = FindObjectOfType<MathParser>();
            turnManager = FindObjectOfType<TurnManagement.TurnManager>();
            playerManager = FindObjectOfType<PlayerManager>();

            if (hudController) hudController.Initialize(turnManager, playerManager);
            if (mathInputPanel)
            {
                mathInputPanel.Initialize(mathParser);
                mathInputPanel.OnConfirmed += expr => OnShootSubmitted?.Invoke(expr);
                mathInputPanel.OnCancelled += () => actionSelector?.Show();
            }

            if (actionSelector)
            {
                actionSelector.OnShootClicked += () =>
                {
                    actionSelector.Hide();
                    mathInputPanel?.Show();
                };
                actionSelector.OnBlinkClicked += () =>
                {
                    OnBlinkSubmitted?.Invoke(Vector2.zero);
                };
                actionSelector.OnRotateClicked += () => OnRotateSubmitted?.Invoke();
            }

            // 主菜单按钮绑定
            if (pvpButton) pvpButton.onClick.AddListener(() => SelectMode(GameMode.PvP));
            if (pvaiButton) pvaiButton.onClick.AddListener(() => SelectMode(GameMode.PvAI));
            if (settingsButton) settingsButton.onClick.AddListener(() => settingsPanel?.Show());
            if (exitButton) exitButton.onClick.AddListener(() => Application.Quit());

            // 难度按钮
            if (easyButton) easyButton.onClick.AddListener(() =>
            { selectedDifficulty = Difficulty.Easy; HighlightDifficultyButton(easyButton); });
            if (normalButton) normalButton.onClick.AddListener(() =>
            { selectedDifficulty = Difficulty.Normal; HighlightDifficultyButton(normalButton); });
            if (hardButton) hardButton.onClick.AddListener(() =>
            { selectedDifficulty = Difficulty.Hard; HighlightDifficultyButton(hardButton); });

            // 监听全局事件
            GameEvent.OnShowMessage += ShowMessage;
            GameEvent.OnGameOver += (winnerID) =>
            {
                var winner = playerManager?.GetPlayer(winnerID);
                if (winner != null) ShowGameOver(winnerID, winner.PlayerName);
            };

            ShowMainMenu();
        }

        private void OnDestroy()
        {
            GameEvent.OnShowMessage -= ShowMessage;
        }

        public void ShowMainMenu()
        {
            mainMenuPanel?.SetActive(true);
            gameHUDPanel?.SetActive(false);
            mathInputPanel?.Hide();
            gameOverPanel?.Hide();
            actionSelector?.Hide();
            settingsPanel?.Hide();

            // 重置选择
            if (difficultySelector) difficultySelector.SetActive(false);
            HighlightDifficultyButton(normalButton);
            selectedMode = GameMode.PvP;
            selectedDifficulty = Difficulty.Normal;
        }

        public void ShowGameHUD()
        {
            mainMenuPanel?.SetActive(false);
            gameHUDPanel?.SetActive(true);
            gameOverPanel?.Hide();
            settingsPanel?.Hide();
        }

        public void RefreshHUD(PlayerState currentPlayer, int turnNumber)
        {
            hudController?.Refresh(turnNumber, currentPlayer);
            // AI玩家不显示动作面板
            if (!currentPlayer.IsAI)
            {
                actionSelector?.SetBlinkAvailable(currentPlayer.BlinkCharges > 0);
                actionSelector?.Show();
            }
            else
            {
                actionSelector?.Hide();
                mathInputPanel?.Hide();
            }
        }

        public void ShowActionPanel()
        {
            var player = playerManager?.GetCurrentPlayer();
            if (player != null && !player.IsAI)
                actionSelector?.Show();
        }
        public void HideActionPanel() => actionSelector?.Hide();
        public void ShowMathInputPanel() => mathInputPanel?.Show();

        public void UpdateCurvePreview(CurveData curve)
        {
            if (curvePreviewLine == null || curve?.Points == null) return;
            curvePreviewLine.positionCount = curve.Points.Count;
            for (int i = 0; i < curve.Points.Count; i++)
                curvePreviewLine.SetPosition(i, curve.Points[i].WorldPos);
        }

        public void ShowMessage(string msg, float duration = 2f)
        {
            if (messageText == null) return;
            if (messageCoroutine != null) StopCoroutine(messageCoroutine);
            messageText.text = msg;
            messageText.gameObject.SetActive(true);
            messageCoroutine = StartCoroutine(FadeMessage(duration));
        }

        private IEnumerator FadeMessage(float duration)
        {
            yield return new WaitForSeconds(duration);
            if (messageText) messageText.gameObject.SetActive(false);
        }

        public void ShowGameOver(int winnerID, string winnerName)
        {
            var winner = playerManager?.GetPlayer(winnerID);
            if (winner != null && gameOverPanel != null)
            {
                gameOverPanel.Show(winner);
                actionSelector?.Hide();
                mathInputPanel?.Hide();
            }
        }

        // ============ 模式选择 ============
        private void SelectMode(GameMode mode)
        {
            selectedMode = mode;
            if (mode == GameMode.PvAI)
            {
                difficultySelector?.SetActive(true);
            }
            else
            {
                difficultySelector?.SetActive(false);
                // PvP直接开始
                OnGameStartRequested?.Invoke(mode, Difficulty.Normal);
            }
        }

        /// <summary>人机模式：选好难度后点击开始</summary>
        public void OnStartAIButtonClicked()
        {
            OnGameStartRequested?.Invoke(GameMode.PvAI, selectedDifficulty);
        }

        private void HighlightDifficultyButton(Button active)
        {
            // 所有难度按钮变灰，选中的保持白色
            ColorBlock normalColors = ColorBlock.defaultColorBlock;
            ColorBlock selectedColors = normalColors;
            selectedColors.normalColor = Color.green;
            selectedColors.selectedColor = Color.green;
            selectedColors.highlightedColor = Color.green;

            if (easyButton) easyButton.colors = easyButton == active ? selectedColors : normalColors;
            if (normalButton) normalButton.colors = normalButton == active ? selectedColors : normalColors;
            if (hardButton) hardButton.colors = hardButton == active ? selectedColors : normalColors;
        }

        // ============ 按钮回调（兼容旧版） ============
        public void OnStartButtonClicked()
        {
            // 兼容旧版：默认PvP
            OnStartGameClicked?.Invoke();
        }
        public void OnShootButtonClicked() => OnShootSubmitted?.Invoke("");
        public void OnBlinkButtonClicked() => OnBlinkSubmitted?.Invoke(Vector2.zero);
        public void OnRotateButtonClicked() => OnRotateSubmitted?.Invoke();
    }
}
