using UnityEngine;

namespace rts.scriptable {
    [CreateAssetMenu]
    public class BuildingTypeSO : ScriptableObject {
        public enum BuildingType {
            None,
            ZombieSpawner,
            SoldierTower,
            SoldierBarracks,
        }
        
        public BuildingType buildingType;
    }
}