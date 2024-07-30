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
            // var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            foreach (var selected in SystemAPI.Query<RefRO<Selected>>()) {
                SystemAPI.GetComponentRW<LocalTransform>(selected.ValueRO.VisualEntity).ValueRW.Scale = 2f;
                // ecb.SetEnabled(selected.ValueRO.VisualEntity, true);
            }
            
            foreach (var selected in SystemAPI.Query<RefRO<Selected>>().WithDisabled<Selected>()) {
                SystemAPI.GetComponentRW<LocalTransform>(selected.ValueRO.VisualEntity).ValueRW.Scale = 0f;
                // ecb.SetEnabled(selected.ValueRO.VisualEntity, false);
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) {

        }
    }
}