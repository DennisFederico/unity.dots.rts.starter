using Unity.Entities;
using UnityEngine;

namespace rts.authoring {
    public class SoldiersHQAuthoring : MonoBehaviour {
        private class SoldiersHQAuthoringBaker : Baker<SoldiersHQAuthoring> {
            public override void Bake(SoldiersHQAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<SoldiersHQ>(entity);
            }
        }
    }
    
    public struct SoldiersHQ : IComponentData {
        
    }
}