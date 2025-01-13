using rts.components;
using Unity.Entities;
using UnityEngine;

namespace rts.authoring {
    public class BuildingAuthoring : MonoBehaviour {
        
        private class BuildingBaker : Baker<BuildingAuthoring> {
            public override void Bake(BuildingAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<BuildingTag>(entity);
            }
        }
    }
}