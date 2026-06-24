using UnityEngine;
using MathGame.Core.Data;

namespace MathGame.Player
{
    public class LevelSystem : MonoBehaviour
    {
        /// <summary>
        /// 处理玩家吃星星
        /// </summary>
        public static void CollectStar(IPlayerManager playerManager, int playerID, StarData star)
        {
            var result = playerManager.AddExp(playerID, star.ExpValue);
            if (result.leveledUp)
            {
                GameEvent.OnShowMessage?.Invoke(
                    $"{playerManager.GetPlayer(playerID)?.PlayerName} 升级到 Lv.{result.newLevel}！");
            }
        }
    }
}
