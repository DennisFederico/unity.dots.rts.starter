using rts.authoring;
using rts.scriptable;
using Unity.Burst;
using Unity.Entities;
using Unity.Rendering;
using IJobEntity = Unity.Entities.IJobEntity;

namespace rts.systems {
    public partial struct ActiveAnimationPlaySystem : ISystem {
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<AnimationDataHolder>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            var animationDataHolder = SystemAPI.GetSingleton<AnimationDataHolder>();

            new ActiveAnimationPlayJob() {
                DeltaTime = SystemAPI.Time.DeltaTime,
                AnimationDataArray = animationDataHolder.AnimationDataArray
            }.ScheduleParallel();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
    }


    [BurstCompile]
    public partial struct ActiveAnimationPlayJob : IJobEntity {
        public float DeltaTime;
        public BlobAssetReference<BlobArray<AnimationData>> AnimationDataArray;

        private void Execute(ref ActiveAnimation activeAnimation, ref MaterialMeshInfo materialMeshInfo) {
            //TODO Why don't keep a reference of the animation data array in the ActiveAnimation component
            ref var animationData = ref AnimationDataArray.Value[(int)activeAnimation.ActiveAnimationType];
            var valueTimerMax = animationData.TimerMax;
            var valueFrameMax = animationData.FrameMax;

            activeAnimation.TimerCurrent += DeltaTime;
            if (!(activeAnimation.TimerCurrent >= valueTimerMax)) return;
            activeAnimation.TimerCurrent -= valueTimerMax;
            activeAnimation.FrameCurrent = (activeAnimation.FrameCurrent + 1) % valueFrameMax;
            materialMeshInfo.MeshID = animationData.MeshIds[activeAnimation.FrameCurrent];

            if (activeAnimation is { FrameCurrent: 0, ActiveAnimationType: AnimationDataSO.AnimationType.ZombieMeleeAttack }) {
                activeAnimation.ActiveAnimationType = AnimationDataSO.AnimationType.None;
            }
        }
    }
}