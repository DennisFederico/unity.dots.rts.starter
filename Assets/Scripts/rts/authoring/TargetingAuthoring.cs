using rts.components;
using Unity.Entities;
using UnityEngine;

namespace rts.authoring {
    public class TargetingAuthoring : MonoBehaviour {
        
        [SerializeField] private float targetingRange;
        [SerializeField] private Faction targetFaction;
        [SerializeField] private LayerMask targetLayers;
        private class TargetingAuthoringBaker : Baker<TargetingAuthoring> {
            public override void Bake(TargetingAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new TargetingData {
                    Range = authoring.targetingRange,
                    TargetLayers = authoring.targetLayers,
                    TargetFaction = authoring.targetFaction
                });
                AddComponent<Target>(entity);
            }
        }
    }
}