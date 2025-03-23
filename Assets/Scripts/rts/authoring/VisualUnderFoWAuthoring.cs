using Unity.Entities;
using UnityEngine;

namespace rts.authoring {
    public class VisualUnderFoWAuthoring : MonoBehaviour {
        
        [SerializeField] private float sphereCastSize;
        [SerializeField] private GameObject parent;
        
        private class VisualUnderFoWAuthoringBaker : Baker<VisualUnderFoWAuthoring> {
            public override void Bake(VisualUnderFoWAuthoring authoring) {
                DependsOn(authoring.parent);
                if (authoring.parent == null) return;
                
                var parentEntity = GetEntity(authoring.parent, TransformUsageFlags.Dynamic);
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                
                AddComponent(entity, new VisualUnderFoW() {
                    IsVisible = true,
                    RootParent = parentEntity,
                    SphereCastSize = authoring.sphereCastSize
                });
            }
        }
    }
    
    public struct VisualUnderFoW : IComponentData {
        public bool IsVisible;
        public Entity RootParent;
        public float SphereCastSize;
    }
}