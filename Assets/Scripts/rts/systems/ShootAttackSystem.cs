using rts.authoring;
using rts.components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
// ReSharper disable PossiblyImpureMethodCallOnReadonlyVariable

namespace rts.systems {
    
    [UpdateAfter(typeof(TargetingSystemGroup))]
    public partial struct ShootAttackSystem : ISystem {
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<EntityReferences>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            var entityReferences = SystemAPI.GetSingleton<EntityReferences>();

            foreach (var (localTransform, attack, target, shouldMove, destination) in 
                     SystemAPI.Query<RefRW<LocalTransform>, RefRW<ShootAttack>, RefRO<Target>, RefRW<ShouldMove>, RefRW<MoveDestination>>()) {
//              SystemAPI.Query <RefRW<LocalTransform>, RefRW<ShootAttack>, RefRO<Target>, RefRW<ShouldMove>, RefRW<MoveDestination>, RefRO<MoveData>>()
                //Are we moving?
                if (shouldMove.ValueRO.Value) continue;

                //On Cool-Down
                attack.ValueRW.CooldownTimer -= SystemAPI.Time.DeltaTime;
                if (attack.ValueRW.CooldownTimer > 0) continue;

                //Have target?
                if (target.ValueRO.Value == Entity.Null) continue;

                //Is Target Valid?
                if (!SystemAPI.HasComponent<LocalTransform>(target.ValueRO.Value)) continue;
                
                //Check distance
                var localPosition = localTransform.ValueRO.Position;
                var targetTransform = SystemAPI.GetComponentRO<LocalTransform>(target.ValueRO.Value);
                var enemyPosition = targetTransform.ValueRO.Position;
                var dirToEnemy = math.normalize(enemyPosition - localPosition);
                // var distanceSquared = math.distancesq(localTransform.ValueRO.Position,  targetLocalTransform.Position);
                float distanceSq = math.distancesq(localPosition,  enemyPosition);
                if (distanceSq > attack.ValueRO.AttackDistanceSquared) {
                    //Target too far - get in range
                    var distanceVector = dirToEnemy * -1 * attack.ValueRO.AttackDistance * .9f;
                    destination.ValueRW.Value = enemyPosition + distanceVector;
                    shouldMove.ValueRW.Value = true;
                } else {
                    //In Range - Rotate to target and Spawn bullet
                    var lookRotation = quaternion.LookRotationSafe(dirToEnemy, math.up());
                    localTransform.ValueRW.Rotation = lookRotation; //TODO SLERP!?

                    //Shoot
                    var bulletEntity = ecb.Instantiate(entityReferences.BulletPrefab);
                    var shootOrigin = localTransform.ValueRO.TransformPoint(attack.ValueRO.ProjectileOffset);
                    var targetPosition = targetTransform.ValueRO.TransformPoint(target.ValueRO.AttackOffset);
                    var shootDirection = math.normalize(targetPosition - shootOrigin);
                    var bulletRotation = quaternion.LookRotationSafe(shootDirection, math.up());
                    ecb.SetComponent(bulletEntity, LocalTransform.FromPositionRotation(shootOrigin, bulletRotation));
                    ecb.SetComponent(bulletEntity, new Target {
                        Value = target.ValueRO.Value,
                        AttackOffset = target.ValueRO.AttackOffset
                    });
                    ecb.SetComponent(bulletEntity, new BulletData() {
                        Speed = 100f,
                        Damage = attack.ValueRO.Damage
                    });
                    
                    var muzzleFlash = ecb.Instantiate(entityReferences.ShootLightPrefab);
                    ecb.SetComponent(muzzleFlash, LocalTransform.FromPositionRotation(shootOrigin, bulletRotation));
                    
                    //Restart Cooldown
                    attack.ValueRW.CooldownTimer = attack.ValueRW.Cooldown;
                }
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) {

        }
    }
}