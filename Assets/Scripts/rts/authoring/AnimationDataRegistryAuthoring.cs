using rts.scriptable;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

namespace rts.authoring {
    public class AnimationDataRegistryAuthoring : MonoBehaviour {
        
        [SerializeField] private AnimationDataListSO animationDataList;
        // "Universal Render Pipeline/Lit"
        //[SerializeField] private Material material = ;
        
        private class AnimationDataRegistryAuthoringBaker : Baker<AnimationDataRegistryAuthoring> {
            public override void Bake(AnimationDataRegistryAuthoring authoring) {
                DependsOn(authoring.animationDataList);

                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent<AnimationDataHolder>(entity);
                
                var animationTypes = System.Enum.GetValues(typeof(AnimationDataSO.AnimationType));
                foreach (AnimationDataSO.AnimationType animationType in animationTypes) {
                    var animationDataSO = authoring.animationDataList.GetAnimationDataSO(animationType);
                    for (var m = 0; m < animationDataSO.meshes.Length; m++) {
                        var meshEntity = CreateAdditionalEntity(TransformUsageFlags.None, true);
                        AddComponent<MaterialMeshInfo>(meshEntity);
                        AddComponent(meshEntity, new RenderMeshUnmanaged() {
                            materialForSubMesh = new Material(Shader.Find("Standard")),
                            mesh = animationDataSO.meshes[m],
                        });
                        AddComponent(meshEntity, new AnimationDataRegistrySubEntity() {
                            AnimationType = animationType,
                            MeshIndex = m,
                        });
                    }
                }
                
                AddComponent(entity, new AnimationDataRegistrySourceSO() {
                    AnimationDataList = authoring.animationDataList,
                });
            }
        }
    }

    public struct AnimationDataRegistrySourceSO : IComponentData {
        public UnityObjectRef<AnimationDataListSO> AnimationDataList;
    }
    
    public struct AnimationDataRegistrySubEntity : IComponentData {
        public AnimationDataSO.AnimationType AnimationType;
        public int MeshIndex;
    }
    
    public struct AnimationDataHolder : IComponentData {
        public BlobAssetReference<BlobArray<AnimationData>> AnimationDataArray;
    }

    public struct AnimationData {
        public float TimerMax;
        public int FrameMax;
        public BlobArray<int> MeshIds;
    }
}

/*
private class AnimationDataRegistryAuthoringBaker : Baker<AnimationDataRegistryAuthoring> {
       public override void Bake(AnimationDataRegistryAuthoring authoring) {
           DependsOn(authoring.animationDataList);

           var entity = GetEntity(TransformUsageFlags.Dynamic);
           var animationDataHolder = new AnimationDataHolder();

           var entitiesGraphicsSystem = World.DefaultGameObjectInjectionWorld
               .GetExistingSystemManaged<EntitiesGraphicsSystem>();

           var blobBuilder = new BlobBuilder(Allocator.Temp);
           ref var animationDataBlobArray = ref blobBuilder.ConstructRoot<BlobArray<AnimationData>>();

           var animationTypes = System.Enum.GetValues(typeof(AnimationDataSO.AnimationType));
           
           var blobBuilderArray = blobBuilder.Allocate(ref animationDataBlobArray, animationTypes.Length);

           var i = 0;
           foreach (AnimationDataSO.AnimationType animationType in animationTypes) {
               var animationDataSO = authoring.animationDataList.GetAnimationDataSO(animationType);
               blobBuilderArray[i].TimerMax = animationDataSO.timerMax;
               blobBuilderArray[i].FrameMax = animationDataSO.meshes.Length;
               
               var meshIds = blobBuilder.Allocate(ref blobBuilderArray[i].MeshIds, animationDataSO.meshes.Length);
               for (var m = 0; m < animationDataSO.meshes.Length; m++) {
                   meshIds[m] = entitiesGraphicsSystem.RegisterMesh(animationDataSO.meshes[m]);
               }

               i++;
           }
           
           animationDataHolder.AnimationDataArray = blobBuilder.CreateBlobAssetReference<BlobArray<AnimationData>>(Allocator.Persistent);
           blobBuilder.Dispose();
           AddBlobAsset(ref animationDataHolder.AnimationDataArray, out _);
           
           AddComponent(entity, animationDataHolder);
       }
   }
   */