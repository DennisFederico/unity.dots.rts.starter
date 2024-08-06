using rts.components;
using Unity.Entities;
using UnityEngine;

namespace rts.authoring {
    public class ShootAttackAuthoring : MonoBehaviour {
        [SerializeField] private float cooldown;
        [SerializeField] private int damage;
        
        private class ShootAttackAuthoringBaker : Baker<ShootAttackAuthoring> {
            public override void Bake(ShootAttackAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new ShootAttack {
                    Cooldown = authoring.cooldown,
                    CooldownTimer = 0,
                    Damage = authoring.damage
                });
            }
        }
    }
}