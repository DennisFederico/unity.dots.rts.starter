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
            foreach (var target in SystemAPI.Query<RefRW<Target>>()) {
                if (target.ValueRO.Value != Entity.Null) continue;
                
                if (!SystemAPI.Exists(target.ValueRO.Value) || !SystemAPI.HasComponent<LocalTransform>(target.ValueRO.Value)) {
                    target.ValueRW.Value = Entity.Null;
                }
            }
        }
    }
}