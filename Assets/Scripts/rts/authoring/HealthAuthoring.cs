using rts.components;
using Unity.Entities;
using UnityEngine;

namespace rts.authoring {
    public class HealthAuthoring : MonoBehaviour {

        [SerializeField] private int healthAmount;
        private class HealthAuthoringBaker : Baker<HealthAuthoring> {
            public override void Bake(HealthAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new Health {
                    Value = authoring.healthAmount,
                });
            }
        }
    }
}