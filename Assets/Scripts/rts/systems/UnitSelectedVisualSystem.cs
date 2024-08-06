using rts.components;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

namespace rts.systems {
    
    public partial struct UnitSelectedVisualSystem : ISystem {
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            //NOTE: Enabling or Disabling entities is a structural change since it adds/remove the Disable component Tag
            //Better to modify the scale of the entities instead of enabling/disabling them
            foreach (var selected in SystemAPI.Query<RefRO<Selected>>().WithDisabled<Selected>()) {
                SystemAPI.GetComponentRW<LocalTransform>(selected.ValueRO.VisualEntity).ValueRW.Scale = 0f;
            }
            
            foreach (var selected in SystemAPI.Query<RefRO<Selected>>()) {
                SystemAPI.GetComponentRW<LocalTransform>(selected.ValueRO.VisualEntity).ValueRW.Scale = selected.ValueRO.ShowScale;
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) {

        }
    }
}