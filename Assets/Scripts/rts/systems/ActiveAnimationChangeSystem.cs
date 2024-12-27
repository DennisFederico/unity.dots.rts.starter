using rts.authoring;
using rts.scriptable;
using Unity.Burst;
using Unity.Entities;
using Unity.Rendering;

namespace rts.systems {
    
    [UpdateBefore(typeof(ActiveAnimationPlaySystem))]
    public partial struct ActiveAnimationChangeSystem : ISystem {
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<AnimationDataHolder>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            foreach (var (activeAnimation, materialMeshInfo) 
                     in SystemAPI.Query<RefRW<ActiveAnimation>, RefRW<MaterialMeshInfo>>()) {
                
                if (activeAnimation.ValueRO.ActiveAnimationType == activeAnimation.ValueRO.NextAnimationType) continue;
                if (activeAnimation.ValueRO.ActiveAnimationType == AnimationDataSO.AnimationType.ZombieMeleeAttack) continue;
                
                activeAnimation.ValueRW.ActiveAnimationType = activeAnimation.ValueRO.NextAnimationType;
                activeAnimation.ValueRW.FrameCurrent = 0;
                activeAnimation.ValueRW.TimerCurrent = 0;
                
                //TODO Why don't keep a reference of the animation data array in the ActiveAnimation component
                var animationDataHolder = SystemAPI.GetSingleton<AnimationDataHolder>();
                ref var animationData = ref animationDataHolder.AnimationDataArray.Value[(int) activeAnimation.ValueRO.ActiveAnimationType];
                materialMeshInfo.ValueRW.MeshID = animationData.MeshIds[activeAnimation.ValueRO.FrameCurrent];
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
    }
}