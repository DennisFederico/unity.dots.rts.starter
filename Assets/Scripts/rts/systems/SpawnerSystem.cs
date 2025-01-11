using rts.authoring;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace rts.systems {
    public partial struct SpawnerSystem : ISystem {
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<PhysicsWorldSingleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            var collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
            var hitList = new NativeList<DistanceHit>(Allocator.Temp);
            
            //EnemyCollision - Zombie = 8
            var collisionFilter = new CollisionFilter {
                BelongsTo = ~0u,
                CollidesWith = (uint) 1 << 8,
                GroupIndex = 0
            };
            
            //TODO Implement a custom collector that can return ONLY the number of hits and AddHits skips once a maximum is reached (earlyOut)
            foreach (var (spawnerData, spawnerPosition) in SystemAPI.Query<RefRW<SpawnerAuthoring.SpawnerData>, RefRO<LocalTransform>>()) {
                spawnerData.ValueRW.SpawnTimer -= SystemAPI.Time.DeltaTime;
                if (!(spawnerData.ValueRO.SpawnTimer <= 0)) continue;
                spawnerData.ValueRW.SpawnTimer = spawnerData.ValueRO.SpawnRate;

                hitList.Clear();
                if (collisionWorld.OverlapSphere(spawnerPosition.ValueRO.Position, spawnerData.ValueRO.MaxSpawnDistance, ref hitList, collisionFilter) &&
                    hitList.Length >= spawnerData.ValueRO.MaxNearbySpawnCount) {
                    
                    continue;
                }
                
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