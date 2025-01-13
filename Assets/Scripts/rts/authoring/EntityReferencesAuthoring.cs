using rts.scriptable;
using Unity.Entities;
using UnityEngine;

namespace rts.authoring {
    public class EntityReferencesAuthoring : MonoBehaviour {
        
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private GameObject scoutPrefab;
        [SerializeField] private GameObject soldierPrefab;
        [SerializeField] private GameObject zombiePrefab;
        [SerializeField] private GameObject shootLightPrefab;
        
        private class EntityReferencesAuthoringBaker : Baker<EntityReferencesAuthoring> {
            public override void Bake(EntityReferencesAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new EntityReferences() {
                    BulletPrefab = GetEntity(authoring.bulletPrefab, TransformUsageFlags.Dynamic),
                    ShootLightPrefab = GetEntity(authoring.shootLightPrefab, TransformUsageFlags.Dynamic),
                    ScoutPrefab = GetEntity(authoring.scoutPrefab, TransformUsageFlags.Dynamic),
                    SoldierPrefab = GetEntity(authoring.soldierPrefab, TransformUsageFlags.Dynamic),
                    ZombiePrefab = GetEntity(authoring.zombiePrefab, TransformUsageFlags.Dynamic),
                });
            }
        }
    }
    
    public struct EntityReferences : IComponentData {
        public Entity BulletPrefab;
        public Entity ShootLightPrefab;
        public Entity ScoutPrefab;
        public Entity SoldierPrefab;
        public Entity ZombiePrefab;
        
        public Entity GetPrefabForType(UnitTypeSO.UnitType unitType) {
            switch (unitType) {
                case UnitTypeSO.UnitType.Scout:
                    return ScoutPrefab;
                case UnitTypeSO.UnitType.Soldier:
                    return SoldierPrefab;
                case UnitTypeSO.UnitType.Zombie:
                    return ZombiePrefab;
                default:
                    return Entity.Null;
            }
        }
    }
}