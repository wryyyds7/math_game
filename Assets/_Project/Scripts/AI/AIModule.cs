using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathGame.Core.Data;
using MathGame.Core.Interfaces;
using MathGame.MathParser;

namespace MathGame.AI
{
    /// <summary>
    /// AI模块 — 控制AI玩家的思考和决策
    /// </summary>
    public class AIModule : MonoBehaviour, IAIModule
    {
        [SerializeField] private DifficultyConfig difficultyConfig;

        private IMathParser mathParser;
        private ISceneManager sceneManager;
        private AIDecisionMaker decisionMaker;
        private CurveStrategy curveStrategy;

        public Difficulty CurrentDifficulty { get; private set; } = Difficulty.Normal;

        private void Awake()
        {
            mathParser = FindObjectOfType<MathParser>();
            sceneManager = FindObjectOfType<SceneManagement.SceneManager>();
        }

        public void Initialize(Difficulty difficulty)
        {
            CurrentDifficulty = difficulty;
            curveStrategy = new CurveStrategy(mathParser);
            decisionMaker = new AIDecisionMaker(difficulty, difficultyConfig);

            Debug.Log($"[AIModule] 初始化完成，难度: {difficulty}");
        }

        public void Think(PlayerState ai, List<PlayerState> allPlayers,
                          List<ObstacleData> obstacles, Action<TurnActionType, object> onActionReady)
        {
            StartCoroutine(ThinkCoroutine(ai, allPlayers, obstacles, onActionReady));
        }

        private IEnumerator ThinkCoroutine(PlayerState ai, List<PlayerState> allPlayers,
                                            List<ObstacleData> obstacles,
                                            Action<TurnActionType, object> onActionReady)
        {
            // 模拟思考延迟
            float thinkDelay = CurrentDifficulty switch
            {
                Difficulty.Easy => difficultyConfig.EasyThinkDelay + UnityEngine.Random.Range(0.2f, 0.5f),
                Difficulty.Normal => difficultyConfig.NormalThinkDelay + UnityEngine.Random.Range(0.1f, 0.3f),
                Difficulty.Hard => difficultyConfig.HardThinkDelay + UnityEngine.Random.Range(0.05f, 0.15f),
                _ => 1f
            };

            yield return new WaitForSeconds(thinkDelay);

            float mw = sceneManager?.MapWidth ?? 1920;
            float mh = sceneManager?.MapHeight ?? 1080;

            var (action, data) = decisionMaker.DecideAction(
                ai, allPlayers, obstacles, curveStrategy, mw, mh);

            onActionReady?.Invoke(action, data);
            Debug.Log($"[AIModule] AI({ai.PlayerName}) 决策: {action}");
        }
    }
}
