using Unity.Entities;
using UnityEngine;

namespace rts.authoring {
    public class EntityReferencesAuthoring : MonoBehaviour {
        
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private GameObject soldierPrefab;
        [SerializeField] private GameObject zombiePrefab;
        [SerializeField] private GameObject shootLightPrefab;
        
        private class EntityReferencesAuthoringBaker : Baker<EntityReferencesAuthoring> {
            public override void Bake(EntityReferencesAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new EntityReferences() {
                    BulletPrefab = GetEntity(authoring.bulletPrefab, TransformUsageFlags.Dynamic),
                    SoldierPrefab = GetEntity(authoring.soldierPrefab, TransformUsageFlags.Dynamic),
                    ZombiePrefab = GetEntity(authoring.zombiePrefab, TransformUsageFlags.Dynamic),
                    ShootLightPrefab = GetEntity(authoring.shootLightPrefab, TransformUsageFlags.Dynamic), 
                });
            }
        }
    }
    
    public struct EntityReferences : IComponentData {
        public Entity BulletPrefab;
        public Entity SoldierPrefab;
        public Entity ZombiePrefab;
        public Entity ShootLightPrefab;
    }
}