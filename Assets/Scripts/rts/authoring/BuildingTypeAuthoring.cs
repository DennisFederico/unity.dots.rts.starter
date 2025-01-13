using rts.scriptable;
using Unity.Entities;
using UnityEngine;

namespace rts.authoring {
    public class BuildingTypeAuthoring : MonoBehaviour {
        
        [SerializeField] private BuildingTypeSO.BuildingType buildingType;
        private class BuildingTypeAuthoringBaker : Baker<BuildingTypeAuthoring> {

            public override void Bake(BuildingTypeAuthoring authoring) {

                var entity = GetEntity(TransformUsageFlags.Dynamic);
                
                AddComponent(entity, new BuildingType {
                    Value = authoring.buildingType
                });

            }
        }
    }

    public struct BuildingType : IComponentData {
        public BuildingTypeSO.BuildingType Value;
    }
}