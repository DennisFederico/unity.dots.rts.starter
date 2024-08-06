using Unity.Burst;
using Unity.Entities;

namespace rts.systems {
    
    [DisableAutoCreation]
    [UpdateInGroup(typeof(LateSimulationSystemGroup), OrderLast = true)]
    public partial struct EventsResetSystem : ISystem {
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            // foreach (var selected in SystemAPI.Query<RefRW<Selected>>().WithPresent<Selected>()) {
            //     selected.ValueRW.OnSelected = false;
            //     selected.ValueRW.OnDeselected = false;
            // }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) {

        }
    }
}