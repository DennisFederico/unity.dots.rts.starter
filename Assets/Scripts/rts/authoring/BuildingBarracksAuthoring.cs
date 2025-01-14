using rts.scriptable;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace rts.authoring {
    public class BuildingBarracksAuthoring : MonoBehaviour {
        private class BuildingBarracksAuthoringBaker : Baker<BuildingBarracksAuthoring> {
            public override void Bake(BuildingBarracksAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new BuildingBarracksState() {
                    ProgressRequired = 2,
                    ActiveUnitType = UnitTypeSO.UnitType.None,
                    RallyPositionOffset = new float3(0, 0, 5)
                });
                AddBuffer<BarrackSpawnBuffer>(entity);
                AddComponent<BuildingBarracksUnitEnqueue>(entity);
                SetComponentEnabled<BuildingBarracksUnitEnqueue>(entity, false);
            }
        }
    }

    public struct BuildingBarracksState : IComponentData {
        public float Progress;
        public float ProgressRequired;
        public UnitTypeSO.UnitType ActiveUnitType;
        public float3 RallyPositionOffset;
        public bool HasQueueChanged;
    }

    [InternalBufferCapacity(10)]
    public struct BarrackSpawnBuffer : IBufferElementData {
        public UnitTypeSO.UnitType Value;
    }
    
    public struct BuildingBarracksUnitEnqueue : IComponentData, IEnableableComponent {
        public UnitTypeSO.UnitType UnitType;
    }
}