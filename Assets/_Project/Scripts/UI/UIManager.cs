using System;
using System.Collections;
using UnityEngine;
using TMPro;
using MathGame.Core.Data;
using MathGame.Core.Interfaces;
using MathGame.Player;

namespace MathGame.UI
{
    public class UIManager : MonoBehaviour, IUIManager
    {
        [Header("面板")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject gameHUDPanel;
        [SerializeField] private MathInputPanel mathInputPanel;
        [SerializeField] private TurnActionSelector actionSelector;
        [SerializeField] private HUDController hudController;
        [SerializeField] private GameOverPanel gameOverPanel;

        [Header("消息提示")]
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private float messageFadeTime = 2f;

        [Header("曲线预览")]
        [SerializeField] private LineRenderer curvePreviewLine;

        // 事件
        public event Action OnStartGameClicked;
        public event Action<string> OnShootSubmitted;
        public event Action<Vector2> OnBlinkSubmitted;
        public event Action OnRotateSubmitted;

        private IMathParser mathParser;
        private ITurnManager turnManager;
        private IPlayerManager playerManager;
        private Coroutine messageCoroutine;

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
                    // 进入地图点击模式选择闪现目标
                    OnBlinkSubmitted?.Invoke(Vector2.zero);
                };
                actionSelector.OnRotateClicked += () => OnRotateSubmitted?.Invoke();
            }

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
        }

        public void ShowGameHUD()
        {
            mainMenuPanel?.SetActive(false);
            gameHUDPanel?.SetActive(true);
            gameOverPanel?.Hide();
        }

        public void RefreshHUD(PlayerState currentPlayer, int turnNumber)
        {
            hudController?.Refresh(turnNumber, currentPlayer);
            actionSelector?.SetBlinkAvailable(currentPlayer.BlinkCharges > 0);
            actionSelector?.Show();
        }

        public void ShowActionPanel() => actionSelector?.Show();
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

        // ============ 按钮回调 ============
        public void OnStartButtonClicked() => OnStartGameClicked?.Invoke();
        public void OnShootButtonClicked() => OnShootSubmitted?.Invoke("");
        public void OnBlinkButtonClicked() => OnBlinkSubmitted?.Invoke(Vector2.zero);
        public void OnRotateButtonClicked() => OnRotateSubmitted?.Invoke();
    }
}
