using rts.authoring;
using rts.components;
using rts.scriptable;
using Unity.Burst;
using Unity.Entities;

namespace rts.systems {
    
    [UpdateBefore(typeof(ActiveAnimationChangeSystem))]
    public partial struct AnimationStateSystem : ISystem {
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {

            foreach (var (_, animatedMeshEntity) in SystemAPI.Query<RefRO<ShouldMove>, RefRO<AnimatedMeshEntity>>()) {
                SystemAPI.GetComponentRW<ActiveAnimation>(animatedMeshEntity.ValueRO.Value).ValueRW.NextAnimationType = AnimationDataSO.AnimationType.SoldierWalk;
            }
            foreach (var animatedMeshEntity in SystemAPI.Query<RefRO<AnimatedMeshEntity>>().WithDisabled<ShouldMove>()) {
                SystemAPI.GetComponentRW<ActiveAnimation>(animatedMeshEntity.ValueRO.Value).ValueRW.NextAnimationType = AnimationDataSO.AnimationType.SoldierIdle;
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
    }
}