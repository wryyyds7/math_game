using System;
using UnityEngine;
using TMPro;
using MathGame.Core.Data;

namespace MathGame.UI
{
    public class GameOverPanel : MonoBehaviour
    {
        [SerializeField] private TMP_Text winnerText;
        [SerializeField] private TMP_Text statsText;
        [SerializeField] private UnityEngine.UI.Button restartBtn;
        [SerializeField] private UnityEngine.UI.Button menuBtn;

        public event Action OnRestartClicked;
        public event Action OnMenuClicked;

        private void Awake()
        {
            if (restartBtn) restartBtn.onClick.AddListener(() => OnRestartClicked?.Invoke());
            if (menuBtn) menuBtn.onClick.AddListener(() => OnMenuClicked?.Invoke());
        }

        public void Show(PlayerState winner)
        {
            gameObject.SetActive(true);
            winnerText.text = $"{winner.PlayerName} 获胜！";
            statsText.text = $"击杀: {winner.Kills} | 等级: Lv.{winner.Level} | 收集: {winner.StarsCollected}颗星";
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
