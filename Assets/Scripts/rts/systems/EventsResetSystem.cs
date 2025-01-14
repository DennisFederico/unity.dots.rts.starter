using rts.authoring;
using rts.components;
using rts.mono;
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

        //[BurstCompile]
        public void OnUpdate(ref SystemState state) {
            jobHandles[0] = new ResetHealthChangeEventJob().ScheduleParallel(state.Dependency);
            jobHandles[1] = new ResetMeleeAttackJob().ScheduleParallel(state.Dependency);
            
            var barracksThatChangedQueue = new NativeList<Entity>(Allocator.TempJob);
            new ResetBuildingBarracksQueueChangedJob() {
                BarracksEntitiesThatChangedList = barracksThatChangedQueue.AsParallelWriter()
            }.ScheduleParallel(state.Dependency).Complete();
            
            state.Dependency = JobHandle.CombineDependencies(jobHandles);
            
            if (barracksThatChangedQueue.Length > 0) {
                DOTSEventManager.Instance.TriggerOnBarracksQueueChanged(barracksThatChangedQueue);
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
    }

    [BurstCompile]
    public partial struct ResetHealthChangeEventJob : IJobEntity {
        private void Execute(ref Health health) {
            health.HasChanged = false;
        }
    }
    
    [BurstCompile]
    public partial struct ResetMeleeAttackJob : IJobEntity {
        private void Execute(ref MeleeAttack meleeAttack) {
            meleeAttack.OnAttack = false;
        }
    }
    
    [BurstCompile]
    public partial struct ResetBuildingBarracksQueueChangedJob : IJobEntity {
        
        public NativeList<Entity>.ParallelWriter BarracksEntitiesThatChangedList;

        private void Execute(ref BuildingBarracksState barrackState, Entity barrackEntity) {
            if (barrackState.HasQueueChanged) {
                BarracksEntitiesThatChangedList.AddNoResize(barrackEntity);
            }
            barrackState.HasQueueChanged = false;
        }
    }
}