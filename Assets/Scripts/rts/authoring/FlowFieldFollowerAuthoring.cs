using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace rts.authoring {
    public class FlowFieldFollowerAuthoring : MonoBehaviour {
        private class FlowFieldFollowerAuthoringBaker : Baker<FlowFieldFollowerAuthoring> {
            public override void Bake(FlowFieldFollowerAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new FlowFieldFollower());
                SetComponentEnabled<FlowFieldFollower>(entity, false);
            }
        }
    }
    
    public struct FlowFieldFollower : IComponentData, IEnableableComponent {
        public float3 LastFlowFieldVector;
        public float3 TargetPosition;
    }
}