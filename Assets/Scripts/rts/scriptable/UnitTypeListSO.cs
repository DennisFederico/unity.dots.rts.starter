using System.Collections.Generic;
using UnityEngine;

namespace rts.scriptable {
    [CreateAssetMenu]
    public class UnitTypeListSO : ScriptableObject {
        public List<UnitTypeSO> unitTypeSOList;
        public UnitTypeSO None;
        
        public UnitTypeSO GetUnitTypeSO(UnitTypeSO.UnitType unitType) {
            foreach (var unitTypeSO in unitTypeSOList) {
                if (unitTypeSO.unitType == unitType) {
                    return unitTypeSO;
                }
            }
            Debug.Log($"Could not find a UnitTypeSO with the UnitType {unitType}");
            return null;
        }
    }
}