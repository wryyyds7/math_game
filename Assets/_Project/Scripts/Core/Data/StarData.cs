using System;
using UnityEngine;

namespace MathGame.Core.Data
{
    [Serializable]
    public class StarData
    {
        public int StarID;
        public StarSize Size;
        public Vector2 Position;
        public bool IsCollected;

        public int ExpValue => Size switch
        {
            StarSize.Small => 10,
            StarSize.Medium => 30,
            StarSize.Large => 80,
            _ => 0
        };

        public float VisualScale => Size switch
        {
            StarSize.Small => 0.5f,
            StarSize.Medium => 0.8f,
            StarSize.Large => 1.2f,
            _ => 0.5f
        };
    }
}
