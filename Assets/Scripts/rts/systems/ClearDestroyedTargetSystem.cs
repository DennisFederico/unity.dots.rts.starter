using rts.components;
using Unity.Burst;
using Unity.Entities;

namespace rts.systems {
    
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public partial struct ClearDestroyedTargetSystem : ISystem {
        
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            
        }


        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            foreach (var target in SystemAPI.Query<RefRW<AttackTarget>>()) {
                if (SystemAPI.Exists(target.ValueRO.Value)) {
                    continue;
                }
                target.ValueRW.Value = Entity.Null;
            }
        }
    }
}