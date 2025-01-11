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
            var animationDataHolder = SystemAPI.GetSingleton<AnimationDataHolder>();
            new ActiveAnimationChangeJob() {
                AnimationDataArray = animationDataHolder.AnimationDataArray
            }.ScheduleParallel();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
    }

    [BurstCompile]
    public partial struct ActiveAnimationChangeJob : IJobEntity {
        public BlobAssetReference<BlobArray<AnimationData>> AnimationDataArray;
        
        private void Execute(ref ActiveAnimation activeAnimation, ref MaterialMeshInfo materialMeshInfo) {
            if (activeAnimation.ActiveAnimationType == activeAnimation.NextAnimationType) return;
            if (AnimationDataSO.IsAnimationUnInterruptible(activeAnimation.ActiveAnimationType)) return;

            activeAnimation.ActiveAnimationType = activeAnimation.NextAnimationType;
            activeAnimation.FrameCurrent = 0;
            activeAnimation.TimerCurrent = 0;

            //TODO Why don't keep a reference of the animation data array in the ActiveAnimation component
            ref var animationData = ref AnimationDataArray.Value[(int)activeAnimation.ActiveAnimationType];
            materialMeshInfo.Mesh = animationData.MeshIds[activeAnimation.FrameCurrent];
        }
    }
}