using rts.scriptable;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Serialization;

namespace rts.authoring {
    public class ActiveAnimationAuthoring : MonoBehaviour {
        
        [SerializeField] private AnimationDataSO.AnimationType nextAnimationType;
        
        private class ActiveAnimationAuthoringBaker : Baker<ActiveAnimationAuthoring> {
            public override void Bake(ActiveAnimationAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new ActiveAnimation() {
                    NextAnimationType = authoring.nextAnimationType,
                });
            }
        }
    }
    
    public struct ActiveAnimation : IComponentData {
        public int FrameCurrent;
        public float TimerCurrent;
        public AnimationDataSO.AnimationType ActiveAnimationType;
        public AnimationDataSO.AnimationType NextAnimationType;
    }
}