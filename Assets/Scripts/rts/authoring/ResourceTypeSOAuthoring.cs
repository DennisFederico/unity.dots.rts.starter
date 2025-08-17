using rts.scriptable;
using Unity.Entities;
using UnityEngine;

namespace rts.authoring {
    public class ResourceTypeSOAuthoring : MonoBehaviour {
        
        [SerializeField] private ResourceTypeSO.ResourceType resourceType;
        
        private class ResourceTypeSOAuthoringBaker : Baker<ResourceTypeSOAuthoring> {
            public override void Bake(ResourceTypeSOAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new ResourceTypeSOComponent() {
                    ResourceType = authoring.resourceType
                });
            }
        }
    }

    public struct ResourceTypeSOComponent : IComponentData {
        public ResourceTypeSO.ResourceType ResourceType;
    }
}