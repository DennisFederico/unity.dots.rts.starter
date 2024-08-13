using rts.authoring;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace rts.systems {
    public partial struct SpawnerSystem : ISystem {
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

            foreach (var spawnerData in SystemAPI.Query<RefRW<SpawnerAuthoring.SpawnerData>>()) {
                spawnerData.ValueRW.SpawnTimer -= SystemAPI.Time.DeltaTime;
                if (!(spawnerData.ValueRO.SpawnTimer <= 0)) continue;
                spawnerData.ValueRW.SpawnTimer = spawnerData.ValueRO.SpawnRate;
                var random = spawnerData.ValueRW.Random.NextFloat2(new float2(-spawnerData.ValueRO.Area.x, -spawnerData.ValueRO.Area.y), new float2(spawnerData.ValueRO.Area.x, spawnerData.ValueRO.Area.y));
                var spawnPosition = spawnerData.ValueRO.Position + new float3(random.x, 0, random.y);
                var spawn = ecb.Instantiate(spawnerData.ValueRO.Prefab);
                ecb.SetComponent(spawn, LocalTransform.FromPosition(spawnPosition));
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) {

        }
    }
}