using System.Collections.Generic;
using UnityEngine;
using MathGame.Core.Data;

namespace MathGame.Core.Interfaces
{
    public interface IPlayerManager
    {
        List<PlayerState> GetAllPlayers();
        PlayerState GetPlayer(int playerID);
        PlayerState GetCurrentPlayer();
        PlayerState SpawnPlayer(string name, bool isAI = false);
        bool TryBlink(int playerID, Vector2 targetPosition);
        void RotatePlayer(int playerID);
        (bool leveledUp, int newLevel) AddExp(int playerID, int exp);
        void EliminatePlayer(int playerID);
        int GetAlivePlayerCount();
        bool IsPlayerAlive(int playerID);
    }
}
