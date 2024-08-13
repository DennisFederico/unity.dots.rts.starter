using rts.components;
using rts.mono;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

namespace rts.systems {
    public partial struct MeleeAttackSystem : ISystem {
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<PhysicsWorldSingleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            foreach (var (localTransform, meleeAttack, target, shouldMove, moveDestination, targetingData) in
                     SystemAPI
                         .Query<RefRW<LocalTransform>, RefRW<MeleeAttack>, RefRO<Target>, RefRW<ShouldMove>, RefRW<MoveDestination>, RefRO<TargetingData>>()) {
                if (target.ValueRO.Value == Entity.Null || !SystemAPI.HasComponent<LocalTransform>(target.ValueRO.Value)) {
                    continue;
                }

                //Check if close enough
                var targetLocalTransform = SystemAPI.GetComponentRO<LocalTransform>(target.ValueRO.Value);
                var distanceSq = math.distancesq(localTransform.ValueRO.Position, targetLocalTransform.ValueRO.Position);
                var closeToTarget = distanceSq <= meleeAttack.ValueRO.AttackDistanceSq;
                
                //Far from target center but on touching distance?
                bool touching = false;
                if (!closeToTarget) {
                    var dirToTarget = math.normalize(targetLocalTransform.ValueRO.Position - localTransform.ValueRO.Position);
                    var touchRay = new RaycastInput() {
                        Start = localTransform.ValueRO.Position,
                        End = localTransform.ValueRO.Position + dirToTarget * meleeAttack.ValueRO.TouchDistance,
                        Filter = new CollisionFilter {
                            BelongsTo = ~0u,
                            CollidesWith = (uint)targetingData.ValueRO.TargetLayers.value,
                            GroupIndex = 0
                        }
                    };

                    var collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
                    if (collisionWorld.CastRay(touchRay, out var closestHit)) {
                        //Double check for the same target?
                        if (closestHit.Entity != target.ValueRO.Value) {
                            Debug.Log($"TARGET ASSERT WARNING, Expecting {target.ValueRO.Value}, Found {closestHit.Entity}");
                        }
                        touching = true;
                    }
                }

                if (!touching && !closeToTarget) {
                    //Target too far
                    shouldMove.ValueRW.Value = true;
                    moveDestination.ValueRW.Value = targetLocalTransform.ValueRO.Position;
                } else {
                    //Target close
                    shouldMove.ValueRW.Value = false;
                    moveDestination.ValueRW.Value = localTransform.ValueRO.Position;
                    meleeAttack.ValueRW.Timer -= SystemAPI.Time.DeltaTime;

                    if (meleeAttack.ValueRO.Timer > 0) {
                        continue;
                    }

                    //Inflict Damage
                    meleeAttack.ValueRW.Timer = meleeAttack.ValueRO.TimerMax;
                    var health = SystemAPI.GetComponentRW<Health>(target.ValueRO.Value);
                    health.ValueRW.ApplyDamage(meleeAttack.ValueRO.Damage);
                }
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) {
        }
    }
}