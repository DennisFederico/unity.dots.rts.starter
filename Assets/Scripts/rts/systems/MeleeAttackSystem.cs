using rts.components;
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
            foreach (var (
                         localTransform, 
                         meleeAttack, 
                         target, 
                         moveDestination, 
                         targetingData, 
                         shouldMove,
                         shouldAttack) in
                     SystemAPI
                         .Query<
                             RefRW<LocalTransform>, 
                             RefRW<MeleeAttack>, 
                             RefRO<Target>, 
                             RefRW<MoveDestination>, 
                             RefRO<TargetingData>, 
                             EnabledRefRW<ShouldMove>,
                             EnabledRefRW<ShouldAttack>
                         >()
                         .WithPresent<ShouldMove, ShouldAttack>()) {
                if (target.ValueRO.Value == Entity.Null || !SystemAPI.HasComponent<LocalTransform>(target.ValueRO.Value)) {
                    continue;
                }

                //Check if close enough
                var targetLocalTransform = SystemAPI.GetComponentRO<LocalTransform>(target.ValueRO.Value);
                var targetPositionWithOffset = targetLocalTransform.ValueRO.Position + target.ValueRO.AttackOffset;
                var localPositionWithOffset = localTransform.ValueRO.Position + target.ValueRO.AttackOffset;
                var distanceSq = math.distancesq(localPositionWithOffset, targetPositionWithOffset);
                var closeToTarget = distanceSq <= meleeAttack.ValueRO.AttackDistanceSq;
                
                Debug.Log($"DistanceSq: {distanceSq}, AttackDistanceSq: {meleeAttack.ValueRO.AttackDistanceSq}, CloseToTarget: {closeToTarget}, ShouldMove: {shouldMove.ValueRO}");
                
                //TODO closeToTarget measures the distance to the center of the target, we need to raycast to check if we are "touching" the target
                if (!closeToTarget) {
                    shouldMove.ValueRW = true;
                    shouldAttack.ValueRW = false;
                    moveDestination.ValueRW.Value = targetLocalTransform.ValueRO.Position;
                } else {
                    Debug.Log($"Close enough to target, distance: {distanceSq} - Real: {math.distance(localPositionWithOffset, targetPositionWithOffset)}");
                    shouldMove.ValueRW = false;
                    shouldAttack.ValueRW = true;
                    moveDestination.ValueRW.Value = localTransform.ValueRO.Position;
                    
                    meleeAttack.ValueRW.Timer -= SystemAPI.Time.DeltaTime;
                    
                    if (meleeAttack.ValueRO.Timer > 0) {
                        continue;
                    }
                    
                    //Inflict Damage
                    //TODO - Add the damage to a buffer, Don't apply here, use a buffer on the target and a system to apply the aggregated damage as multiple entities can hit the same target
                    meleeAttack.ValueRW.Timer = meleeAttack.ValueRO.TimerMax;
                    var health = SystemAPI.GetComponentRW<Health>(target.ValueRO.Value);
                    health.ValueRW.ApplyDamage(meleeAttack.ValueRO.Damage);
                    
                    meleeAttack.ValueRW.OnAttack = true;
                }
                
                Debug.Log($"After - ShouldMove: {shouldMove.ValueRO}");

                // TO CHECK IF TOUCHING THE TARGET
                //     var dirToTarget = math.normalize(targetPositionWithOffset - localPositionWithOffset);
                //     var touchRay = new RaycastInput() {
                //         Start = localPositionWithOffset,
                //         End = localPositionWithOffset + (dirToTarget * meleeAttack.ValueRO.TouchDistance),
                //         Filter = new CollisionFilter {
                //             BelongsTo = ~0u,
                //             CollidesWith = (uint)targetingData.ValueRO.TargetLayers.value,
                //             GroupIndex = 0
                //         }
                //     };
                //
                //     var collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
                //     if (collisionWorld.CastRay(touchRay, out var closestHit)) {
                //         //Double check for the same target?
                //         if (closestHit.Entity != target.ValueRO.Value) {
                //             //Debug.Log($"TARGET ASSERT WARNING, Expecting {target.ValueRO.Value}, Found {closestHit.Entity}");
                //             continue;
                //         }
                //         touching = true;
                //     }
                //

            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) {
        }
    }
}