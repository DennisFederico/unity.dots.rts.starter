using Unity.Entities;
using UnityEngine;

namespace rts.authoring {
    public class BuildingBarracksAuthoring : MonoBehaviour {
        private class BuildingBarracksAuthoringBaker : Baker<BuildingBarracksAuthoring> {
            public override void Bake(BuildingBarracksAuthoring authoring) {
                
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new BuildingBarracksState() {});
                
            }
            
        }
    }

    public struct BuildingBarracksState : IComponentData {
        public float Progress;
        public float ProgressRequired;
    }
    
}