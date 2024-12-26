using rts.scriptable;
using Unity.Entities;
using UnityEngine;

namespace rts.authoring {
    public class AnimatedMeshAuthoring : MonoBehaviour {
        
        [SerializeField] private GameObject gameObjectMesh;
        [SerializeField] private AnimationDataSO.AnimationType idleAnimationType;
        [SerializeField] private AnimationDataSO.AnimationType walkAnimationType;
        
        private class AnimatedMeshAuthoringBaker : Baker<AnimatedMeshAuthoring> {
            public override void Bake(AnimatedMeshAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new AnimatedMeshEntity() {
                    Value = GetEntity(authoring.gameObjectMesh, TransformUsageFlags.Dynamic)
                });
                AddComponent(entity, new UnitAnimations() {
                    IdleAnimationType = authoring.idleAnimationType,
                    WalkAnimationType = authoring.walkAnimationType
                });
            }
        }
    }
    
    public struct AnimatedMeshEntity : IComponentData {
        public Entity Value;
    }
    
    public struct UnitAnimations : IComponentData {
        public AnimationDataSO.AnimationType IdleAnimationType;
        public AnimationDataSO.AnimationType WalkAnimationType;
    }
}