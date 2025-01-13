﻿using System.Collections.Generic;
using UnityEngine;

namespace rts.scriptable {
    [CreateAssetMenu]
    public class BuildingTypeListSO : ScriptableObject {
        public List<BuildingTypeSO> buildingTypeSOList;
        
        
        public BuildingTypeSO GetBuildingTypeSO(BuildingTypeSO.BuildingType buildingType) {
            foreach (var buildingTypeSO in buildingTypeSOList) {
                if (buildingTypeSO.buildingType == buildingType) {
                    return buildingTypeSO;
                }
            }
            Debug.Log($"Could not find a BuildingTypeSO with the BuildingType {buildingType}");
            return null;
        }
    }
}