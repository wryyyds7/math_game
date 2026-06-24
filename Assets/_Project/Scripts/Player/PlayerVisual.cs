using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MathGame.Core.Data;

namespace MathGame.Player
{
    public class PlayerVisual : MonoBehaviour
    {
        [SerializeField] private TMP_Text nameLabel;
        [SerializeField] private TMP_Text levelLabel;
        [SerializeField] private Slider expBar;
        [SerializeField] private GameObject[] blinkLights;
        [SerializeField] private SpriteRenderer bodyRenderer;
        [SerializeField] private GameObject deathEffect;

        private PlayerState state;

        public void Initialize(PlayerState playerState)
        {
            state = playerState;
            if (nameLabel) nameLabel.text = playerState.PlayerName;
            if (bodyRenderer && !string.IsNullOrEmpty(playerState.PlayerName))
            {
                // 根据玩家名哈希生成颜色
                int hash = playerState.PlayerName.GetHashCode();
                bodyRenderer.color = Color.HSVToRGB(
                    Mathf.Abs(hash % 360) / 360f, 0.7f, 0.9f);
            }
            Refresh();
        }

        public void Refresh()
        {
            if (state == null) return;

            if (levelLabel) levelLabel.text = $"Lv.{state.Level}";
            if (expBar && state.ExpToNextLevel > 0)
                expBar.value = (float)state.CurrentExp / state.ExpToNextLevel;

            for (int i = 0; i < blinkLights.Length; i++)
                if (blinkLights[i]) blinkLights[i].SetActive(i < state.BlinkCharges);

            if (!state.IsAlive && deathEffect)
            {
                deathEffect.SetActive(true);
                if (bodyRenderer) bodyRenderer.color = new Color(0.3f, 0.3f, 0.3f, 0.5f);
            }
        }
    }
}
