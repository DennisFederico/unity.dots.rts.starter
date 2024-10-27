using rts.components;
using Unity.Entities;
using UnityEngine;

namespace rts.authoring {
    public class LoseTargetDistanceAuthoring : MonoBehaviour {
        [SerializeField] private float distance;

        public class LoseTargetDistanceBaker : Baker<LoseTargetDistanceAuthoring> {
            public override void Bake(LoseTargetDistanceAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new LoseTargetDistance { Value = authoring.distance });
            }
        }
    }
}