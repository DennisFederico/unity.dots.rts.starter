using rts.scriptable;
using Unity.Entities;
using UnityEngine;

namespace rts.authoring {
    public class ActiveAnimationAuthoring : MonoBehaviour {
        
        private class ActiveAnimationAuthoringBaker : Baker<ActiveAnimationAuthoring> {
            public override void Bake(ActiveAnimationAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new ActiveAnimation() {
        
                });
            }
        }
    }
    
    public struct ActiveAnimation : IComponentData {
        public int FrameCurrent;
        public float TimerCurrent;
        public AnimationDataSO.AnimationType ActiveAnimationType;
    }
}