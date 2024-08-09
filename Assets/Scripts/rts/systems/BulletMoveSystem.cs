using rts.components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace rts.systems {
    
    [UpdateAfter(typeof(ShootAttackSystem))]
    public partial struct BulletMoveSystem : ISystem {
        
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            
            foreach (var (bulletTransform, bulletData, target, bulletEntity) in 
                     SystemAPI.Query<RefRW<LocalTransform>, RefRO<BulletData>, RefRO<Target>>()
                         .WithEntityAccess()) {

                
                if (target.ValueRO.Value == Entity.Null || !SystemAPI.HasComponent<LocalTransform>(target.ValueRO.Value)) {
                    ecb.DestroyEntity(bulletEntity);
                    continue;
                }
                
                var targetTransform = SystemAPI.GetComponentRO<LocalTransform>(target.ValueRO.Value);
                // ReSharper disable once PossiblyImpureMethodCallOnReadonlyVariable
                var targetPosition = targetTransform.ValueRO.TransformPoint(target.ValueRO.AttackOffset);
                var bulletPosition = bulletTransform.ValueRO.Position;
                var direction = math.normalize(targetPosition - bulletPosition);
                
                var newBulletPosition = bulletTransform.ValueRW.Position + (direction * bulletData.ValueRO.Speed * SystemAPI.Time.DeltaTime);

                //Close enough to deal damage
                var prevDistanceSq = math.distancesq(bulletPosition, targetPosition);
                var newDistanceSq = math.distancesq(newBulletPosition, targetPosition);
                if (newDistanceSq < 0.1f || newDistanceSq > prevDistanceSq) {
                    bulletTransform.ValueRW.Position = targetPosition;
                    var health = SystemAPI.GetComponentRW<Health>(target.ValueRO.Value);
                    health.ValueRW.Value -= bulletData.ValueRO.Damage;
                    ecb.DestroyEntity(bulletEntity);
                } else {
                    bulletTransform.ValueRW.Position = newBulletPosition;
                }
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) {

        }
    }
}