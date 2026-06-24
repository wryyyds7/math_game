using UnityEngine;

namespace MathGame.Core.Data
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "MathGame/Game Config")]
    public class GameConfig : ScriptableObject
    {
        [Header("地图设置")]
        public float MapWidth = 1920f;
        public float MapHeight = 1080f;
        public int ObstacleCount = 15;
        public float ObstacleMinRadius = 30f;
        public float ObstacleMaxRadius = 80f;
        public int StarCount = 20;
        public float StarGenerateRadius = 40f;

        [Header("玩家设置")]
        public float PlayerRadius = 20f;
        public float PlayerSpawnMargin = 100f;

        [Header("射击设置")]
        public float BulletSpeed = 300f;
        public float CurveSampleStep = 5f;
        public float MaxCurveLength = 1500f;
        public int BaseDamage = 50;

        [Header("伤害半径")]
        public float BaseDamageRadius = 0.5f;
        public float DamageRadiusPerLevel = 0.15f;

        [Header("闪现设置")]
        public float BlinkRangeRatio = 0.3f;
        public int MaxBlinkCharges = 2;

        [Header("等级设置")]
        public int MaxLevel = 20;
        public float ExpCurvePower = 1.5f;
        public int BaseExpForLevel2 = 100;
    }
}
