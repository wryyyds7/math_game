using System.Collections.Generic;
using UnityEngine;
using MathGame.Core.Data;

namespace MathGame.Core.Interfaces
{
    public interface IShootingSystem
    {
        void FireBullet(int ownerID, CurveData curve);
        List<BulletInfo> GetActiveBullets();
        List<Vector2> GetBulletPreview(CurveData curve);
        bool IsAnyBulletFlying { get; }
        void ClearAllBullets();
    }
}
