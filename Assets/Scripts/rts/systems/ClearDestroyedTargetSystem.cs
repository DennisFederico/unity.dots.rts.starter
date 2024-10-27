using rts.components;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

namespace rts.systems {
    
    /// <summary>
    /// Clears any target that has been destroyed on the previous frame by EndSimulationEntityCommandBufferSystem
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
    [UpdateAfter(typeof(EndSimulationEntityCommandBufferSystem))]
    public partial struct ClearDestroyedTargetSystem : ISystem {
        
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            foreach (var targetOverride in SystemAPI.Query<RefRW<TargetOverride>>()) {
                if (targetOverride.ValueRO.Value == Entity.Null || !SystemAPI.Exists(targetOverride.ValueRO.Value) || !SystemAPI.HasComponent<LocalTransform>(targetOverride.ValueRO.Value)) {
                    targetOverride.ValueRW.Value = Entity.Null;
                }
            }
            
            foreach (var target in SystemAPI.Query<RefRW<Target>>()) {
                if (target.ValueRO.Value == Entity.Null || !SystemAPI.Exists(target.ValueRO.Value) || !SystemAPI.HasComponent<LocalTransform>(target.ValueRO.Value)) {
                    target.ValueRW.Value = Entity.Null;
                }
            }
        }
    }
}