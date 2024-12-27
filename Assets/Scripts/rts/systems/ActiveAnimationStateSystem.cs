using rts.authoring;
using rts.components;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;

namespace rts.systems {
    
    [UpdateBefore(typeof(ActiveAnimationChangeSystem))]
    public partial struct ActiveAnimationStateSystem : ISystem {
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {

            foreach (var (_, animatedMeshEntity, unitAnimations) in 
                     SystemAPI.Query<RefRO<ShouldMove>, RefRO<AnimatedMeshEntity>, RefRO<UnitAnimations>>()) {
                SystemAPI.GetComponentRW<ActiveAnimation>(animatedMeshEntity.ValueRO.Value).ValueRW.NextAnimationType = unitAnimations.ValueRO.WalkAnimationType;
            }
            
            foreach (var (_, animatedMeshEntity, unitAnimations) in 
                     SystemAPI.Query<RefRO<ShouldAttack>, RefRO<AnimatedMeshEntity>, RefRO<UnitAnimations>>().WithDisabled<ShouldMove>()) {
                SystemAPI.GetComponentRW<ActiveAnimation>(animatedMeshEntity.ValueRO.Value).ValueRW.NextAnimationType = unitAnimations.ValueRO.AttackAnimationType;
            }

            foreach (var (animatedMeshEntity, unitAnimations) in 
                     SystemAPI.Query<RefRO<AnimatedMeshEntity>, RefRO<UnitAnimations>>().WithDisabled<ShouldMove, ShouldAttack>()) {
                SystemAPI.GetComponentRW<ActiveAnimation>(animatedMeshEntity.ValueRO.Value).ValueRW.NextAnimationType = unitAnimations.ValueRO.IdleAnimationType;
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
    }
}