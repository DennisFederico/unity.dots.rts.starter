using rts.components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace rts.systems {
    public partial struct MeleeAttackSystem : ISystem {
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<PhysicsWorldSingleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            foreach (var (localTransform, meleeAttack, target, moveDestination, targetingData, shouldMove) in
                     SystemAPI
                         .Query<RefRW<LocalTransform>, RefRW<MeleeAttack>, RefRO<Target>, RefRW<MoveDestination>, RefRO<TargetingData>, EnabledRefRW<ShouldMove>>()
                         .WithPresent<ShouldMove>()) {
                if (target.ValueRO.Value == Entity.Null || !SystemAPI.HasComponent<LocalTransform>(target.ValueRO.Value)) {
                    continue;
                }

                //Check if close enough
                var targetLocalTransform = SystemAPI.GetComponentRO<LocalTransform>(target.ValueRO.Value);
                var targetPositionWithOffset = targetLocalTransform.ValueRO.Position + target.ValueRO.AttackOffset;
                var localPositionWithOffset = localTransform.ValueRO.Position + target.ValueRO.AttackOffset;
                var distanceSq = math.distancesq(localPositionWithOffset, targetPositionWithOffset);
                var closeToTarget = distanceSq <= meleeAttack.ValueRO.AttackDistanceSq;
                
                //Far from target center but on touching distance?
                bool touching = false;
                if (!closeToTarget) {
                    var dirToTarget = math.normalize(targetLocalTransform.ValueRO.Position - localPositionWithOffset);
                    var touchRay = new RaycastInput() {
                        Start = localPositionWithOffset,
                        End = localPositionWithOffset + (dirToTarget * meleeAttack.ValueRO.TouchDistance),
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
                            //Debug.Log($"TARGET ASSERT WARNING, Expecting {target.ValueRO.Value}, Found {closestHit.Entity}");
                            continue;
                        }
                        touching = true;
                    }
                }

                if (!touching && !closeToTarget) {
                    //Target too far
                    // state.EntityManager.SetComponentEnabled<ShouldMove>(entity, true);
                    shouldMove.ValueRW = true;
                    moveDestination.ValueRW.Value = targetLocalTransform.ValueRO.Position;
                } else {
                    //Target close
                    // state.EntityManager.SetComponentEnabled<ShouldMove>(entity, false);
                    shouldMove.ValueRW = false;
                    moveDestination.ValueRW.Value = localTransform.ValueRO.Position;
                    meleeAttack.ValueRW.Timer -= SystemAPI.Time.DeltaTime;

                    if (meleeAttack.ValueRO.Timer > 0) {
                        continue;
                    }

                    //Inflict Damage
                    meleeAttack.ValueRW.Timer = meleeAttack.ValueRO.TimerMax;
                    var health = SystemAPI.GetComponentRW<Health>(target.ValueRO.Value);
                    health.ValueRW.ApplyDamage(meleeAttack.ValueRO.Damage);
                    
                    meleeAttack.ValueRW.OnAttack = true;
                }
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) {
        }
    }
}