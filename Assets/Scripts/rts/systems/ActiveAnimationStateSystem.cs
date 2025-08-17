using rts.authoring;
using rts.components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace rts.systems {
    
    [UpdateBefore(typeof(ActiveAnimationChangeSystem))]
    public partial struct ActiveAnimationStateSystem : ISystem {
        
        private ComponentLookup<ActiveAnimation> activeAnimationLookup;
        
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            activeAnimationLookup = state.GetComponentLookup<ActiveAnimation>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            
            activeAnimationLookup.Update(ref state);
            
            new ActiveAnimationStateIdleJob() {
                ActiveAnimationLookup = activeAnimationLookup
            }.ScheduleParallel();
            
            new ActiveAnimationStateWalkingJob() {
                ActiveAnimationLookup = activeAnimationLookup
            }.ScheduleParallel();
            
            new ActiveAnimationStateShootAttackJob() {
                ActiveAnimationLookup = activeAnimationLookup
            }.ScheduleParallel();
            
            new ActiveAnimationStateMeleeAttackJob() {
                ActiveAnimationLookup = activeAnimationLookup
            }.ScheduleParallel();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
    }
    
    [BurstCompile]
    [WithDisabled(typeof(ShouldMove), typeof(ShouldAttack))]
    public partial struct ActiveAnimationStateIdleJob : IJobEntity {
        
        [NativeDisableParallelForRestriction] public ComponentLookup<ActiveAnimation> ActiveAnimationLookup;
        
        private void Execute(in AnimatedMeshEntity animatedMeshEntity, in UnitAnimations unitAnimations) {
            ActiveAnimationLookup.GetRefRWOptional(animatedMeshEntity.Value).ValueRW.NextAnimationType = unitAnimations.IdleAnimationType;
        }
    }
    
    [BurstCompile]
    [WithAll(typeof(ShouldMove))]
    [WithDisabled(typeof(ShouldAttack))]
    public partial struct ActiveAnimationStateWalkingJob : IJobEntity {
        
        [NativeDisableParallelForRestriction] public ComponentLookup<ActiveAnimation> ActiveAnimationLookup;
        
        private void Execute(in AnimatedMeshEntity animatedMeshEntity, in UnitAnimations unitAnimations) {
            ActiveAnimationLookup.GetRefRWOptional(animatedMeshEntity.Value).ValueRW.NextAnimationType = unitAnimations.WalkAnimationType;
        }
    }

    [BurstCompile]
    [WithAll(typeof(ShouldAttack))]
    [WithDisabled(typeof(ShouldMove))]
    public partial struct ActiveAnimationStateShootAttackJob : IJobEntity {
        
        [NativeDisableParallelForRestriction] public ComponentLookup<ActiveAnimation> ActiveAnimationLookup;
        
        private void Execute(in AnimatedMeshEntity animatedMeshEntity, in UnitAnimations unitAnimations) {
            ActiveAnimationLookup.GetRefRWOptional(animatedMeshEntity.Value).ValueRW.NextAnimationType = unitAnimations.ShootAnimationType;
        }
    }
    
    [BurstCompile]
    [WithPresent]
    // [WithAll(typeof(MeleeAttack))]
    [WithDisabled(typeof(ShouldMove))]
    public partial struct ActiveAnimationStateMeleeAttackJob : IJobEntity {
        
        [NativeDisableParallelForRestriction] public ComponentLookup<ActiveAnimation> ActiveAnimationLookup;
        
        private void Execute(in MeleeAttack meleeAttack, in AnimatedMeshEntity animatedMeshEntity, in UnitAnimations unitAnimations) {
            if (!meleeAttack.OnAttack) return;
            ActiveAnimationLookup.GetRefRWOptional(animatedMeshEntity.Value).ValueRW.NextAnimationType = unitAnimations.MeleeAnimationType;
        }
    }
}