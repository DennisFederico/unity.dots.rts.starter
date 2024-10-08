using UnityEngine;

namespace rts.mono {
    public class GameConstants : MonoBehaviour {
        public const int UnitsLayer = 6;
        public const int SoldiersLayer = 7;
        public const int ZombiesLayer = 8;
        public const uint Selectable = 1 << UnitsLayer | 1 << SoldiersLayer;
        
        public static GameConstants Instance { get; private set; }

        private void Awake() {
            Instance = this;
        }
    }
}