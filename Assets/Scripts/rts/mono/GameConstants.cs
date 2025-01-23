using rts.scriptable;
using UnityEngine;

namespace rts.mono {
    public class GameConstants : MonoBehaviour {
        public const int UnitsLayer = 6;
        public const int SoldiersLayer = 7;
        public const int ZombiesLayer = 8;
        public const int BuildingsLayer = 9;
        public const int ObstaclesLayer = 10;
        public const uint Selectable = 1u << UnitsLayer | 1u << SoldiersLayer;
        public const uint StaticObstacles = 1u << BuildingsLayer | 1u << ObstaclesLayer;
        public const uint Zombie = 1u << UnitsLayer | 1u << ZombiesLayer;

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