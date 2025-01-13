using UnityEngine;

namespace rts.scriptable {
    [CreateAssetMenu]
    public class UnitTypeSO : ScriptableObject {

        public enum UnitType {
            None,
            Scout,
            Soldier,
            Zombie,
        }
        
        public UnitType unitType;
        public float buildTimeMax;
    }
}