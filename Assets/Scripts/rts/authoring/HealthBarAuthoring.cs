using Unity.Entities;
using UnityEngine;

namespace rts.authoring {
    public class HealthBarAuthoring : MonoBehaviour {
        
        //TODO Find a way to constraint the use to entities with Health component
        [SerializeField] private GameObject belongsTo;
        [SerializeField] private GameObject bar;
        
        private class HealthBarAuthoringBaker : Baker<HealthBarAuthoring> {
            public override void Bake(HealthBarAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new HealthBar {
                    BelongsTo = GetEntity(authoring.belongsTo, TransformUsageFlags.Dynamic),
                    Bar = GetEntity(authoring.bar, TransformUsageFlags.NonUniformScale),
                });
            }
        }
    }
    
    public struct HealthBar : IComponentData {
        public Entity BelongsTo;
        public Entity Bar;
    }
}