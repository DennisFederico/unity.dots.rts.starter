using rts.components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace rts.systems {
    [UpdateInGroup(typeof(LateSimulationSystemGroup), OrderLast = true)]
    public partial struct EventsResetSystem : ISystem {

        private NativeArray<JobHandle> jobHandles;
        
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            jobHandles = new NativeArray<JobHandle>(2, Allocator.Persistent);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            jobHandles[0] = new ResetHealthChangeEventJob().ScheduleParallel(state.Dependency);
            jobHandles[1] = new ResetMeleeAttackJob().ScheduleParallel(state.Dependency);
            state.Dependency = JobHandle.CombineDependencies(jobHandles);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
    }

    [BurstCompile]
    public partial struct ResetHealthChangeEventJob : IJobEntity {
        public void Execute(ref Health health) {
            health.HasChanged = false;
        }
    }
    
    [BurstCompile]
    public partial struct ResetMeleeAttackJob : IJobEntity {
        public void Execute(ref MeleeAttack meleeAttack) {
            meleeAttack.OnAttack = false;
        }
    }
}