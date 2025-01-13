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
                var buffer = AddBuffer<BarrackSpawnBuffer>(entity);
                buffer.Add(new BarrackSpawnBuffer() { Value = UnitTypeSO.UnitType.Scout });
                buffer.Add(new BarrackSpawnBuffer() { Value = UnitTypeSO.UnitType.Scout });
                buffer.Add(new BarrackSpawnBuffer() { Value = UnitTypeSO.UnitType.Scout });
                buffer.Add(new BarrackSpawnBuffer() { Value = UnitTypeSO.UnitType.Soldier });
                buffer.Add(new BarrackSpawnBuffer() { Value = UnitTypeSO.UnitType.Soldier });
                buffer.Add(new BarrackSpawnBuffer() { Value = UnitTypeSO.UnitType.Scout });
                buffer.Add(new BarrackSpawnBuffer() { Value = UnitTypeSO.UnitType.Zombie });
                buffer.Add(new BarrackSpawnBuffer() { Value = UnitTypeSO.UnitType.Scout });
            }
        }
    }

    public struct BuildingBarracksState : IComponentData {
        public float Progress;
        public float ProgressRequired;
        public UnitTypeSO.UnitType ActiveUnitType;
        public float3 RallyPositionOffset;
    }

    [InternalBufferCapacity(10)]
    public struct BarrackSpawnBuffer : IBufferElementData {
        public UnitTypeSO.UnitType Value;
    }
}