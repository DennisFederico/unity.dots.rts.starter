using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace rts.authoring {
    public class FlowFieldPathRequestAuthoring : MonoBehaviour {
        private class FlowFieldPathRequestAuthoringBaker : Baker<FlowFieldPathRequestAuthoring> {
            public override void Bake(FlowFieldPathRequestAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<FlowFieldPathRequest>(entity);
                SetComponentEnabled<FlowFieldPathRequest>(entity, false);
            }
        }
    }
    
    public struct FlowFieldPathRequest : IComponentData, IEnableableComponent {
        public float3 TargetPosition;
    }
}