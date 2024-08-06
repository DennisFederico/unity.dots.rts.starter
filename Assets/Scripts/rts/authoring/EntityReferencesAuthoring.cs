using Unity.Entities;
using UnityEngine;

namespace rts.authoring {
    public class EntityReferencesAuthoring : MonoBehaviour {
        
        [SerializeField] GameObject bulletPrefab;
        [SerializeField] GameObject soldierPrefab;
        [SerializeField] GameObject zombiePrefab;
        
        private class EntityReferencesAuthoringBaker : Baker<EntityReferencesAuthoring> {
            public override void Bake(EntityReferencesAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new EntityReferences() {
                    BulletPrefab = GetEntity(authoring.bulletPrefab, TransformUsageFlags.Dynamic),
                    SoldierPrefab = GetEntity(authoring.soldierPrefab, TransformUsageFlags.Dynamic),
                    ZombiePrefab = GetEntity(authoring.zombiePrefab, TransformUsageFlags.Dynamic)
                });
            }
        }
    }
    
    public struct EntityReferences : IComponentData {
        public Entity BulletPrefab;
        public Entity SoldierPrefab;
        public Entity ZombiePrefab;
    }
}