using Unity.Burst;
using Unity.Entities;

namespace rts.systems {
    
    [DisableAutoCreation]
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    [UpdateBefore(typeof(EventsResetSystem))]
    public partial struct EventBasedUnitSelectedVisualSystem : ISystem {
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            // foreach (var selected in SystemAPI.Query<RefRO<Selected>>().WithPresent<Selected>()) {
            //     if (selected.ValueRO.OnSelected) {
            //         var selectionVisualTransform = SystemAPI.GetComponentRW<LocalTransform>(selected.ValueRO.VisualEntity);
            //         selectionVisualTransform.ValueRW.Scale = selected.ValueRO.ShowScale;
            //     }
            //     if (selected.ValueRO.OnDeselected) {
            //         var selectionVisualTransform = SystemAPI.GetComponentRW<LocalTransform>(selected.ValueRO.VisualEntity);
            //         selectionVisualTransform.ValueRW.Scale = 0f;
            //     }
            // }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) {

        }
    }
}