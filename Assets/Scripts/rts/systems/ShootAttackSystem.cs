using rts.authoring;
using rts.components;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

namespace rts.systems {
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

            foreach (var (localTransform, attack, target) in 
                     SystemAPI.Query <RefRO<LocalTransform>, RefRW<ShootAttack>, RefRO<AttackTarget>>()) {
                
                attack.ValueRW.CooldownTimer -= SystemAPI.Time.DeltaTime;
                
                if (attack.ValueRW.CooldownTimer > 0) {
                    continue;
                }
                
                if (target.ValueRO.Value == Entity.Null) {
                    continue;
                }
                
                attack.ValueRW.CooldownTimer = attack.ValueRW.Cooldown;

                var bulletEntity = ecb.Instantiate(entityReferences.BulletPrefab);
                ecb.SetComponent(bulletEntity, LocalTransform.FromPosition(localTransform.ValueRO.Position));
                ecb.SetComponent(bulletEntity, new AttackTarget {
                    Value = target.ValueRO.Value
                });
                ecb.SetComponent(bulletEntity, new BulletData() {
                    Speed = 100f,
                    Damage = attack.ValueRO.Damage
                });
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) {

        }
    }
}