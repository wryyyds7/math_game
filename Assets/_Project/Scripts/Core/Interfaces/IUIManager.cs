using System;
using UnityEngine;
using MathGame.Core.Data;

namespace MathGame.Core.Interfaces
{
    public interface IUIManager
    {
        void ShowMainMenu();
        void ShowGameHUD();
        void RefreshHUD(PlayerState currentPlayer, int turnNumber);
        void ShowActionPanel();
        void HideActionPanel();
        void ShowMathInputPanel();
        void UpdateCurvePreview(CurveData curve);
        void ShowMessage(string message, float duration = 2f);
        void ShowGameOver(int winnerID, string winnerName);

        event Action OnStartGameClicked;
        event Action<string> OnShootSubmitted;
        event Action<Vector2> OnBlinkSubmitted;
        event Action OnRotateSubmitted;
    }
}
