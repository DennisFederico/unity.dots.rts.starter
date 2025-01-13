using rts.authoring;
using rts.components;
using rts.mono;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace rts.systems {
    public partial struct BuildingBarracksSystem : ISystem {
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<EntityReferences>();
        }

        //[BurstCompile]
        public void OnUpdate(ref SystemState state) {
            var entityReferences = SystemAPI.GetSingleton<EntityReferences>();
            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

            foreach (var (buildingState, localTransform, spawnBuffer) in
                     SystemAPI.Query<
                         RefRW<BuildingBarracksState>,
                         RefRO<LocalTransform>,
                         DynamicBuffer<BarrackSpawnBuffer>>()) {
                
                if (spawnBuffer.IsEmpty) continue;
                
                buildingState.ValueRW.Progress += SystemAPI.Time.DeltaTime;
                
                if (buildingState.ValueRW.Progress < buildingState.ValueRW.ProgressRequired) continue;
                
                buildingState.ValueRW.Progress = 0;
                
                var nextUnitType = spawnBuffer[0].Value;
                spawnBuffer.RemoveAt(0);
                //Change the active unit type if its different from the current one
                if (nextUnitType != buildingState.ValueRO.ActiveUnitType) {
                    buildingState.ValueRW.ActiveUnitType = nextUnitType;
                    //TODO Bake SO into a BlobAssetReference to keep all burstable
                    buildingState.ValueRW.ProgressRequired = GameConstants.Instance.UnitTypeListSO.GetUnitTypeSO(buildingState.ValueRO.ActiveUnitType).buildTimeMax;
                }
                
                var prefabForType = entityReferences.GetPrefabForType(buildingState.ValueRO.ActiveUnitType);
                var spawn = ecb.Instantiate(prefabForType);
                ecb.SetComponent(spawn, LocalTransform.FromPosition(localTransform.ValueRO.Position));
                ecb.SetComponent(spawn, new MoveDestination { Value = localTransform.ValueRO.Position + buildingState.ValueRO.RallyPositionOffset });
                // ecb.SetComponent(spawn, LocalTransform.FromPosition(localTransform.ValueRO.Position + buildingState.ValueRO.RallyPositionOffset));
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
    }
}