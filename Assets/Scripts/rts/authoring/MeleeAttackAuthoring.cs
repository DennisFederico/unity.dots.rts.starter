using rts.components;
using Unity.Entities;
using UnityEngine;

namespace rts.authoring {
    
    [RequireComponent(typeof(Collider))]
    public class MeleeAttackAuthoring : MonoBehaviour {
        [SerializeField] private float timerMax;
        [SerializeField] private float attackDistance;
        [SerializeField] private int damage;

        public class MeleeAttackBaker : Baker<MeleeAttackAuthoring> {
            public override void Bake(MeleeAttackAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                var collider = authoring.transform.GetComponent<Collider>();

                AddComponent(entity, new MeleeAttack {
                    Timer = authoring.timerMax, 
                    TimerMax = authoring.timerMax,
                    TouchDistance = collider.bounds.size.x + .1f,
                    AttackDistanceSq = authoring.attackDistance * authoring.attackDistance,
                    Damage = authoring.damage
                });
            }
        }
    }
}