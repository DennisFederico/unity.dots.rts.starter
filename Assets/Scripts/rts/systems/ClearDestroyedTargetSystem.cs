using rts.components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace rts.systems {
    
    /// <summary>
    /// Clears any target that has been destroyed on the previous frame by EndSimulationEntityCommandBufferSystem
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
    [UpdateAfter(typeof(EndSimulationEntityCommandBufferSystem))]
    public partial struct ClearDestroyedTargetSystem : ISystem {
        
        private ComponentLookup<LocalTransform> localTransformsLookup;
        private EntityStorageInfoLookup entityStorageInfoLookup;
        
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            localTransformsLookup = state.GetComponentLookup<LocalTransform>();
            entityStorageInfoLookup = state.GetEntityStorageInfoLookup();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            
            localTransformsLookup.Update(ref state);
            entityStorageInfoLookup.Update(ref state);

            new ResetDestroyedTargetJob {
                LocalTransformsLookup = localTransformsLookup,
                EntityLookup = entityStorageInfoLookup
            }.ScheduleParallel();
            
            // MOVED TO A JOB
            // foreach (var target in SystemAPI.Query<RefRW<Target>>()) {
            //     if (target.ValueRO.Value == Entity.Null || !SystemAPI.Exists(target.ValueRO.Value) || !SystemAPI.HasComponent<LocalTransform>(target.ValueRO.Value)) {
            //         target.ValueRW.Value = Entity.Null;
            //     }
            // }
            
            new ResetDestroyedTargetOverrideJob {
                LocalTransformsLookup = localTransformsLookup,
                EntityLookup = entityStorageInfoLookup
            }.ScheduleParallel();
            
            // MOVED TO A JOB
            // foreach (var targetOverride in SystemAPI.Query<RefRW<TargetOverride>>()) {
            //     if (targetOverride.ValueRO.Value == Entity.Null || !SystemAPI.Exists(targetOverride.ValueRO.Value) || !SystemAPI.HasComponent<LocalTransform>(targetOverride.ValueRO.Value)) {
            //         targetOverride.ValueRW.Value = Entity.Null;
            //     }
            // }
        }
    }
    
    [BurstCompile]
    public partial struct ResetDestroyedTargetJob : IJobEntity {

        [ReadOnly] public ComponentLookup<LocalTransform> LocalTransformsLookup;
        [ReadOnly] public EntityStorageInfoLookup EntityLookup;
        public void Execute(ref Target target) {
            if (!EntityLookup.Exists(target.Value) || !LocalTransformsLookup.HasComponent(target.Value)) {
                target.Value = Entity.Null;
            }
        }
    }
    
    public partial struct ResetDestroyedTargetOverrideJob : IJobEntity {

        [ReadOnly] public ComponentLookup<LocalTransform> LocalTransformsLookup;
        [ReadOnly] public EntityStorageInfoLookup EntityLookup;
        public void Execute(ref TargetOverride targetOverride) {
            if (!EntityLookup.Exists(targetOverride.Value) || !LocalTransformsLookup.HasComponent(targetOverride.Value)) {
                targetOverride.Value = Entity.Null;
            }
        }
    }
}