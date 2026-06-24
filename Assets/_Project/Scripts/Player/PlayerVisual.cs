using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MathGame.Core.Data;

namespace MathGame.Player
{
    /// <summary>
    /// 玩家可视化组件 — V2增强：不同玩家不同颜色和形状
    /// </summary>
    public class PlayerVisual : MonoBehaviour
    {
        [Header("标签")]
        [SerializeField] private TMP_Text nameLabel;
        [SerializeField] private TMP_Text levelLabel;
        [SerializeField] private Slider expBar;

        [Header("闪现指示器")]
        [SerializeField] private GameObject[] blinkLights;

        [Header("外观")]
        [SerializeField] private SpriteRenderer bodyRenderer;
        [SerializeField] private SpriteRenderer[] secondaryRenderers; // 装饰部件
        [SerializeField] private GameObject deathEffect;

        [Header("朝向指示器")]
        [SerializeField] private SpriteRenderer directionArrow; // 朝向箭头

        // 预定义玩家颜色（6种不同颜色）
        private static readonly Color[] PlayerColors = new[]
        {
            new Color(0.2f, 0.6f, 1f),    // 蓝色
            new Color(1f, 0.3f, 0.3f),    // 红色
            new Color(0.2f, 0.9f, 0.3f),  // 绿色
            new Color(1f, 0.8f, 0.1f),    // 金色
            new Color(0.8f, 0.3f, 1f),    // 紫色
            new Color(0.1f, 0.9f, 0.9f),  // 青色
        };

        // 预定义AI颜色
        private static readonly Color[] AIColors = new[]
        {
            new Color(1f, 0.4f, 0.15f),   // 橙色
            new Color(0.5f, 0.5f, 0.5f),  // 灰色
            new Color(0.7f, 0.1f, 0.3f),  // 暗红
        };

        private PlayerState state;
        private int colorIndex;

        public void Initialize(PlayerState playerState)
        {
            state = playerState;
            if (nameLabel) nameLabel.text = playerState.PlayerName;

            // 根据玩家ID分配不同颜色
            colorIndex = playerState.PlayerID % (playerState.IsAI ? AIColors.Length : PlayerColors.Length);
            Color assignedColor = playerState.IsAI
                ? AIColors[colorIndex]
                : PlayerColors[colorIndex];

            // 设置主精灵颜色
            if (bodyRenderer)
            {
                bodyRenderer.color = assignedColor;
            }

            // 设置装饰部件颜色（略淡）
            if (secondaryRenderers != null)
            {
                foreach (var sr in secondaryRenderers)
                {
                    if (sr != null)
                        sr.color = Color.Lerp(assignedColor, Color.white, 0.3f);
                }
            }

            // 设置朝向箭头颜色
            if (directionArrow)
            {
                directionArrow.color = playerState.IsAI
                    ? new Color(1f, 0.4f, 0.1f, 0.7f)  // AI: 橙色半透明
                    : new Color(assignedColor.r, assignedColor.g, assignedColor.b, 0.7f);
            }

            // AI标签特殊处理
            if (playerState.IsAI && nameLabel)
            {
                nameLabel.color = new Color(1f, 0.7f, 0.3f); // 金色字
            }

            Refresh();
        }

        public void Refresh()
        {
            if (state == null) return;

            if (levelLabel) levelLabel.text = $"Lv.{state.Level}";
            if (expBar && state.ExpToNextLevel > 0)
                expBar.value = (float)state.CurrentExp / state.ExpToNextLevel;

            // 闪现指示器
            for (int i = 0; i < blinkLights.Length; i++)
                if (blinkLights[i]) blinkLights[i].SetActive(i < state.BlinkCharges);

            // 死亡效果
            if (!state.IsAlive && deathEffect)
            {
                deathEffect.SetActive(true);
                if (bodyRenderer) bodyRenderer.color = new Color(0.3f, 0.3f, 0.3f, 0.5f);
            }

            // 朝向箭头始终指向玩家朝向
            if (directionArrow)
            {
                float angle = state.Rotation - 90f;
                directionArrow.transform.rotation = Quaternion.Euler(0, 0, angle);
            }
        }

        /// <summary>获取当前分配给该玩家的颜色</summary>
        public Color GetPlayerColor()
        {
            if (state == null) return Color.white;
            return state.IsAI
                ? AIColors[colorIndex % AIColors.Length]
                : PlayerColors[colorIndex % PlayerColors.Length];
        }
    }
}
