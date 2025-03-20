using rts.scriptable;
using UnityEngine;

namespace rts.mono {
    public class GameConstants : MonoBehaviour {
        public const int UNITS_LAYER = 6;
        public const int SOLDIERS_LAYER = 7;
        public const int ZOMBIES_LAYER = 8;
        public const int BUILDINGS_LAYER = 9;
        public const int OBSTACLES_LAYER = 10;
        public const int PATHFINDING_HEAVY_LAYER = 11;
        public const uint SELECTABLE = 1u << UNITS_LAYER | 1u << SOLDIERS_LAYER;
        public const uint PATHFINDING_OBSTACLES = 1u << OBSTACLES_LAYER;
        public const uint PATHFINDING_HEAVY = 1u << PATHFINDING_HEAVY_LAYER;
        public const uint ZOMBIE = 1u << UNITS_LAYER | 1u << ZOMBIES_LAYER;

        [SerializeField] private UnitTypeListSO unitTypeListSO;
        [SerializeField] private BuildingTypeListSO buildingTypeListSO;
        
        public UnitTypeListSO UnitTypeListSO => Instance.unitTypeListSO;
        public BuildingTypeListSO BuildingTypeListSO => Instance.buildingTypeListSO;
        
        public static GameConstants Instance { get; private set; }

        private void Awake() {
            Instance = this;
        }
    }
}