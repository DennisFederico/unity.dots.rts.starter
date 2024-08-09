using rts.components;
using Unity.Entities;
using UnityEngine;

namespace rts.authoring {
    public class ShootAttackAuthoring : MonoBehaviour {
        [SerializeField] private float cooldown;
        [SerializeField] private float attackDistance;
        [SerializeField] private int damage;
        [SerializeField] private Transform projectileOrigin;
        
        private class ShootAttackAuthoringBaker : Baker<ShootAttackAuthoring> {
            public override void Bake(ShootAttackAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new ShootAttack {
                    Cooldown = authoring.cooldown,
                    CooldownTimer = 0,
                    AttackDistance = authoring.attackDistance,
                    AttackDistanceSquared = authoring.attackDistance * authoring.attackDistance,
                    ProjectileOffset = authoring.projectileOrigin.localPosition,
                    Damage = authoring.damage
                });
            }
        }
    }
}