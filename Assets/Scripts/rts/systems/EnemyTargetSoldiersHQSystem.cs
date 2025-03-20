using rts.authoring;
using rts.components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;

namespace rts.systems {
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(FixedStepSimulationSystemGroup))] //Because Move Systems are in PhysicsSystemGroup
    public partial struct EnemyTargetSoldiersHQSystem : ISystem {
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<SoldiersHQ>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            var soldiersHQ = SystemAPI.GetSingletonEntity<SoldiersHQ>();
            var hqPosition = SystemAPI.GetComponent<LocalTransform>(soldiersHQ).Position;

            //Don't set the target just the destination
            foreach (var (target,
                         targetPositionQueued,
                         enabledTargetPositionQueued,
                         entity
                         ) in SystemAPI.Query<
                             RefRO<Target>,
                             RefRW<TargetPositionQueued>,
                             EnabledRefRW<TargetPositionQueued>>()
                         .WithPresent<TargetPositionQueued, ShouldMove>()
                         .WithAll<TargetSoldiersHQOnSpawn>()
                         .WithEntityAccess()
                    ) {
                if (target.ValueRO.Value != Entity.Null) continue;
                targetPositionQueued.ValueRW.Value = hqPosition;
                enabledTargetPositionQueued.ValueRW = true;
                ecb.RemoveComponent<TargetSoldiersHQOnSpawn>(entity);
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
    }
}