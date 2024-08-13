using Unity.Entities;
using UnityEngine;

namespace rts.authoring {
    public class TimedDestroyAuthoring : MonoBehaviour {
        
        [SerializeField] private float timeToLive;
        private class TimedDestroyAuthoringBaker : Baker<TimedDestroyAuthoring> {
            public override void Bake(TimedDestroyAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new TimeToLiveDuration { Value = authoring.timeToLive });
            }
        }
    }
    
    public struct TimeToLiveDuration : IComponentData {
        public float Value;
    }
}