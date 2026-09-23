// BoxSpawnPoint.cs

using UnityEngine;

namespace _Game.Scripts.Gameplay
{
    [System.Serializable]
    public struct BoxSpawnPoint {
        public Transform point;
        public bool      unlocked;
    }
}