using rts.authoring;
using rts.components;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

namespace rts.systems {
    
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
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
            foreach (var (target, shouldMove, destination, entity) in
                     SystemAPI.Query<RefRO<Target>,
                             EnabledRefRW<ShouldMove>,
                             RefRW<MoveDestination>>()
                         .WithAll<TargetSoldiersHQOnSpawn>()
                         .WithEntityAccess()) {
                if (target.ValueRO.Value != Entity.Null) continue;
                destination.ValueRW = new MoveDestination { Value = hqPosition };
                shouldMove.ValueRW = true;
                ecb.SetComponentEnabled<ShouldMove>(entity, true);
                ecb.RemoveComponent<TargetSoldiersHQOnSpawn>(entity);
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
    }
}