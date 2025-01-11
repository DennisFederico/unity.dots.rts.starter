using System.Collections.Generic;
using rts.authoring;
using rts.scriptable;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;

namespace rts.systems {
    
    [WorldSystemFilter(WorldSystemFilterFlags.BakingSystem)]
    [UpdateInGroup(typeof(PostBakingSystemGroup))]
    public partial struct AnimationDataRegistryBakingSystem : ISystem {
        
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<AnimationDataHolder>();
            state.RequireForUpdate<AnimationDataRegistrySourceSO>();
        }

        public void OnUpdate(ref SystemState state) {
            
            var animationDataListSO = SystemAPI.GetSingleton<AnimationDataRegistrySourceSO>().AnimationDataList.Value;
            
            // Build a temporary structuring to hold the AnimationData, since BlobArrays are refs that must be allocated on the spot
            Dictionary<AnimationDataSO.AnimationType, int[]> animationDataDict = new();
            
            var animationTypes = System.Enum.GetValues(typeof(AnimationDataSO.AnimationType));
            foreach (AnimationDataSO.AnimationType animationType in animationTypes) {
                var animationDataSO = animationDataListSO.GetAnimationDataSO(animationType);
                animationDataDict[animationType] = new int[animationDataSO.meshes.Length];
            }

            foreach (var (registrySubEntity, 
                         materialMeshInfo) in 
                     SystemAPI.Query<
                         RefRO<AnimationDataRegistrySubEntity>, 
                         RefRO<MaterialMeshInfo>
                     >()) {
                animationDataDict[registrySubEntity.ValueRO.AnimationType][registrySubEntity.ValueRO.MeshIndex] = materialMeshInfo.ValueRO.Mesh;
            }

            //Build the BlobAsset
            var blobBuilder = new BlobBuilder(Allocator.Temp);
            ref var animationDataBlobArray = ref blobBuilder.ConstructRoot<BlobArray<AnimationData>>();
            var blobBuilderArray = blobBuilder.Allocate(ref animationDataBlobArray, animationTypes.Length);
            
            foreach (AnimationDataSO.AnimationType animationType in animationTypes) {
                var animationDataSO = animationDataListSO.GetAnimationDataSO(animationType);
                var index = (int) animationType;
                blobBuilderArray[index].TimerMax = animationDataSO.timerMax;
                blobBuilderArray[index].FrameMax = animationDataSO.meshes.Length;
               
                var meshIds = blobBuilder.Allocate(ref blobBuilderArray[index].MeshIds, animationDataSO.meshes.Length);
                for (var m = 0; m < animationDataDict[animationType].Length; m++) {
                    meshIds[m] = animationDataDict[animationType][m];
                }
            }
            
            var animationDataHolder = SystemAPI.GetSingletonRW<AnimationDataHolder>();
            animationDataHolder.ValueRW.AnimationDataArray = blobBuilder.CreateBlobAssetReference<BlobArray<AnimationData>>(Allocator.Persistent);
            blobBuilder.Dispose();
        }
    }
}