using rts.authoring;
using Unity.Burst;
using Unity.Entities;
using Unity.Rendering;

namespace rts.systems {
    public partial struct ActiveAnimationSystem : ISystem {
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<AnimationDataHolder>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            var animationDataHolder = SystemAPI.GetSingleton<AnimationDataHolder>();

            foreach (var (activeAnimation, materialMeshInfo)
                     in SystemAPI.Query<RefRW<ActiveAnimation>, RefRW<MaterialMeshInfo>>()) {

                //TODO Why don't keep a reference of the animation data array in the ActiveAnimation component
                ref var animationData = ref animationDataHolder.AnimationDataArray.Value[(int) activeAnimation.ValueRO.ActiveAnimationType];
                var valueTimerMax = animationData.TimerMax;
                var valueFrameMax = animationData.FrameMax;
                
                activeAnimation.ValueRW.TimerCurrent += SystemAPI.Time.DeltaTime;
                if (!(activeAnimation.ValueRO.TimerCurrent >= valueTimerMax)) continue;
                activeAnimation.ValueRW.TimerCurrent -= valueTimerMax;
                activeAnimation.ValueRW.FrameCurrent = (activeAnimation.ValueRO.FrameCurrent + 1) % valueFrameMax;
                materialMeshInfo.ValueRW.MeshID = animationData.MeshIds[activeAnimation.ValueRO.FrameCurrent];
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
    }
}