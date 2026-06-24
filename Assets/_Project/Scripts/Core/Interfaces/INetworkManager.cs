using System.Collections.Generic;
using MathGame.Core.Data;

namespace MathGame.Core.Interfaces
{
    public interface INetworkManager
    {
        bool IsConnected { get; }
        bool IsHost { get; }
        void CreateRoom(string roomName, int maxPlayers);
        void JoinRoom(string roomCode);
        void LeaveRoom();
        List<NetworkPlayerInfo> GetRoomPlayers();
        void StartNetworkGame();
        void SubmitNetworkAction(TurnActionType action, string actionData);
    }

    [System.Serializable]
    public struct NetworkPlayerInfo
    {
        public ulong ClientID;
        public string PlayerName;
        public bool IsReady;
    }
}
