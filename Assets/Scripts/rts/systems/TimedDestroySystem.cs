using rts.authoring;
using Unity.Burst;
using Unity.Entities;

namespace rts.systems {
    public partial struct TimedDestroySystem : ISystem {
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            foreach (var (duration, entity) in SystemAPI.Query<RefRW<TimeToLiveDuration>>().WithEntityAccess()) {
                duration.ValueRW.Value -= SystemAPI.Time.DeltaTime;
                if (duration.ValueRO.Value <= 0) {
                    ecb.DestroyEntity(entity);
                }
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) {

        }
    }
}