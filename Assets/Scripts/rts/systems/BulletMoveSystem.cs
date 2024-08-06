using rts.components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace rts.systems {
    public partial struct BulletMoveSystem : ISystem {
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            foreach (var (bulletTransform, bulletData, target, bulletEntity) in 
                     SystemAPI.Query<RefRW<LocalTransform>, RefRO<BulletData>, RefRO<AttackTarget>>()
                         .WithEntityAccess()) {

                if (!SystemAPI.Exists(target.ValueRO.Value)) {
                    ecb.DestroyEntity(bulletEntity);
                    continue;
                }
                
                var targetTransform = SystemAPI.GetComponentRO<LocalTransform>(target.ValueRO.Value);
                var prevDistanceSq = math.distancesq(bulletTransform.ValueRO.Position, targetTransform.ValueRO.Position);
                var direction = math.normalize(targetTransform.ValueRO.Position - bulletTransform.ValueRO.Position);
                bulletTransform.ValueRW.Position += direction * bulletData.ValueRO.Speed * SystemAPI.Time.DeltaTime;
                
                //Close enough to deal damage
                var newDistanceSq = math.distancesq(bulletTransform.ValueRO.Position, targetTransform.ValueRO.Position);
                if (newDistanceSq < 0.1f || newDistanceSq > prevDistanceSq) {
                    var health = SystemAPI.GetComponentRW<Health>(target.ValueRO.Value);
                    health.ValueRW.Value -= bulletData.ValueRO.Damage;
                    ecb.DestroyEntity(bulletEntity);
                }
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) {

        }
    }
}