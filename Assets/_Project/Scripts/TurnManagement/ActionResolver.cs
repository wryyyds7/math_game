using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathGame.Core.Data;
using MathGame.Core.Interfaces;

namespace MathGame.TurnManagement
{
    public class ActionResolver
    {
        private readonly Queue<(int playerID, TurnActionType action, object data)> queue = new();

        // 这些引用在TurnManager中设置
        public IPlayerManager PlayerManager;
        public IShootingSystem ShootingSystem;
        public IMathParser MathParserInstance;
        public GameConfig Config;

        public void QueueAction(int playerID, TurnActionType action, object data)
        {
            queue.Enqueue((playerID, action, data));
        }

        public IEnumerator ResolveAction(PlayerState player)
        {
            while (queue.Count > 0)
            {
                var (pid, action, data) = queue.Dequeue();

                switch (action)
                {
                    case TurnActionType.Shoot:
                        string expr = data as string ?? "y=x";
                        var curve = MathParserInstance.ParseAndGenerate(
                            expr, player.Position, player.Rotation, Config.MaxCurveLength);

                        if (curve.IsValid)
                        {
                            ShootingSystem.FireBullet(pid, curve);
                            GameEvent.OnShowMessage?.Invoke(
                                $"{player.PlayerName} 发射曲线: {expr}");
                        }
                        else
                        {
                            GameEvent.OnShowMessage?.Invoke(
                                $"曲线无效: {curve.ErrorMessage}");
                        }
                        break;

                    case TurnActionType.Blink:
                        Vector2 target = (data is Vector2 v) ? v : player.Position;
                        PlayerManager.TryBlink(pid, target);
                        break;

                    case TurnActionType.Rotate180:
                        PlayerManager.RotatePlayer(pid);
                        break;
                }
                yield return null;
            }
        }
    }
}
