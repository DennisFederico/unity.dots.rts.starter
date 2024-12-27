using rts.components;
using Unity.Burst;
using Unity.Entities;

namespace rts.systems {
    [UpdateInGroup(typeof(LateSimulationSystemGroup), OrderLast = true)]
    public partial struct EventsResetSystem : ISystem {
        [BurstCompile]
        public void OnCreate(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            new ResetHealthChangeEventJob().ScheduleParallel();
            new ResetMeleeAttackJob().ScheduleParallel();
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