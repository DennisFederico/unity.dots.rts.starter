using Unity.Burst;
using Unity.Entities;

namespace rts.systems {
    
    [UpdateInGroup(typeof(LateSimulationSystemGroup), OrderLast = true)]
    [UpdateBefore(typeof(EventsResetSystem))]
    public partial struct HealthDeadSystem : ISystem {
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            var ecs = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            foreach (var (health, entity) in SystemAPI.Query<RefRW<components.Health>>().WithEntityAccess()) {
                if (health.ValueRO.Value > 0) {
                    continue;
                }

                health.ValueRW.OnDead = true;
                ecs.DestroyEntity(entity);
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) {

        }
    }
}