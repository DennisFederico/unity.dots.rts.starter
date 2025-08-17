using rts.components;
using Unity.Entities;
using Unity.Mathematics;
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
                var minDistance = collider.bounds.size.x + .5f;
                
                AddComponent(entity, new MeleeAttack {
                    Timer = authoring.timerMax, 
                    TimerMax = authoring.timerMax,
                    //Remember that the squared value of a number < 1 is lesser than the number itself
                    AttackDistanceSq = AdjustedSquaredDistance(math.max(minDistance, authoring.attackDistance)),
                    Damage = authoring.damage
                });
                AddComponent<ShouldAttack>(entity);
                SetComponentEnabled<ShouldAttack>(entity, false);
            }
            
            private float AdjustedSquaredDistance(float distance) {
                return distance switch {
                    >= 1f => distance * distance,
                    > 0f => math.pow(distance, 0.5f),
                    _ => 0f
                };
            }
        }
    }
}