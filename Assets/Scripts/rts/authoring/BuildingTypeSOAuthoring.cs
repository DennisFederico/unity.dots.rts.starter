using rts.scriptable;
using Unity.Entities;
using UnityEngine;

namespace rts.authoring {
    public class BuildingTypeSOAuthoring : MonoBehaviour {
        [SerializeField] private BuildingTypeSO.BuildingType buildingType;

        private class BuildingTypeAuthoringBaker : Baker<BuildingTypeSOAuthoring> {
            public override void Bake(BuildingTypeSOAuthoring authoring) {
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