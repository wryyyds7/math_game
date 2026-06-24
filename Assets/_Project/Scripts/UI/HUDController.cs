using UnityEngine;
using TMPro;
using MathGame.Core.Data;
using MathGame.Core.Interfaces;

namespace MathGame.UI
{
    public class HUDController : MonoBehaviour
    {
        [SerializeField] private TMP_Text turnText;
        [SerializeField] private TMP_Text currentPlayerText;
        [SerializeField] private TMP_Text countdownText;
        [SerializeField] private UnityEngine.UI.Slider expBar;
        [SerializeField] private UnityEngine.UI.Image[] blinkLights;

        private ITurnManager turnManager;
        private IPlayerManager playerManager;

        public void Initialize(ITurnManager tm, IPlayerManager pm)
        {
            turnManager = tm;
            playerManager = pm;
        }

        private void Update()
        {
            if (turnManager == null || turnManager.CurrentPhase != GamePhase.PlayerTurn) return;

            float remaining = turnManager.GetTurnTimeRemaining();
            if (countdownText != null && remaining > 0)
            {
                countdownText.text = $"{Mathf.CeilToInt(remaining)}s";
                countdownText.color = remaining <= 10 ? Color.red : Color.white;
            }
        }

        public void Refresh(int turnNumber, PlayerState currentPlayer)
        {
            if (currentPlayerText) currentPlayerText.text = $"当前: {currentPlayer.PlayerName}";
            if (turnText) turnText.text = $"回合 {turnNumber}";
            if (expBar) expBar.value = currentPlayer.ExpToNextLevel > 0
                ? (float)currentPlayer.CurrentExp / currentPlayer.ExpToNextLevel : 1f;

            for (int i = 0; i < blinkLights.Length; i++)
            {
                if (blinkLights[i] != null)
                    blinkLights[i].color = i < currentPlayer.BlinkCharges ? Color.white : Color.gray;
            }
        }

        public void Show() => gameObject.SetActive(true);
        public void Hide() => gameObject.SetActive(false);
    }
}
