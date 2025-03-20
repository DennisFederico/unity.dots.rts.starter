using Unity.Entities;
using UnityEngine;

namespace rts.authoring {
    public class TargetPositionQueuedAuthoring : MonoBehaviour {
        private class TargetPositionQueuedAuthoringBaker : Baker<TargetPositionQueuedAuthoring> {
            public override void Bake(TargetPositionQueuedAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<TargetPositionQueued>(entity);
                SetComponentEnabled<TargetPositionQueued>(entity, false);
            }
        }
    }
    
    public struct TargetPositionQueued : IComponentData, IEnableableComponent {
        public Vector3 Value;
    }
}