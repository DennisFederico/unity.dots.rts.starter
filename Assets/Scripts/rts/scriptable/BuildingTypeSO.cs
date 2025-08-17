using UnityEngine;

namespace rts.scriptable {
    [CreateAssetMenu]
    public class BuildingTypeSO : ScriptableObject {
        public enum BuildingType {
            None,
            ZombieSpawner,
            SoldierTower,
            SoldierBarracks,
            SoldiersHQ,
            IronHarvester,
            GoldHarvester,
            OilHarvester,
        }
        
        public BuildingType buildingType;
        public Transform prefab; //User for building placement
        public Transform buildingGhost;
        public Sprite sprite; //Used for UI
        public bool showInBuildUI;
        
        public bool IsNone => buildingType == BuildingType.None;
    }
}