using rts.authoring;
using rts.components;
using rts.mono;
using Unity.Burst;
using Unity.Entities;
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

            foreach (var (enqueueContent, enqueue, barrackState, spawnBuffer) in
                     SystemAPI.Query<RefRO<BuildingBarracksUnitEnqueue>, 
                         EnabledRefRW<BuildingBarracksUnitEnqueue>, 
                         RefRW<BuildingBarracksState>, 
                         DynamicBuffer<BarrackSpawnBuffer>>()) {
                spawnBuffer.Add(new BarrackSpawnBuffer { Value = enqueueContent.ValueRO.UnitType });
                enqueue.ValueRW = false;
                barrackState.ValueRW.HasQueueChanged = true;
            }
            
            foreach (var (buildingState, localTransform, spawnBuffer) in
                     SystemAPI.Query<
                         RefRW<BuildingBarracksState>,
                         RefRO<LocalTransform>,
                         DynamicBuffer<BarrackSpawnBuffer>>()) {
                
                if (spawnBuffer.IsEmpty) continue;
                
                if (buildingState.ValueRO.ActiveUnitType != spawnBuffer[0].Value) {
                    buildingState.ValueRW.ActiveUnitType = spawnBuffer[0].Value;
                    //TODO Bake SO into a BlobAssetReference to keep all burstable
                    buildingState.ValueRW.ProgressRequired = GameConstants.Instance.UnitTypeListSO.GetUnitTypeSO(spawnBuffer[0].Value).buildTimeMax;
                }
                
                buildingState.ValueRW.Progress += SystemAPI.Time.DeltaTime;
                if (buildingState.ValueRW.Progress < buildingState.ValueRW.ProgressRequired) continue;
                
                buildingState.ValueRW.Progress = 0;
                spawnBuffer.RemoveAt(0);
                buildingState.ValueRW.HasQueueChanged = true;
                
                var prefabForType = entityReferences.GetPrefabForType(buildingState.ValueRO.ActiveUnitType);
                var spawn = ecb.Instantiate(prefabForType);
                ecb.SetComponent(spawn, LocalTransform.FromPosition(localTransform.ValueRO.Position));
                ecb.SetComponent(spawn, new MoveDestination { Value = localTransform.ValueRO.Position + buildingState.ValueRO.RallyPositionOffset });
                // TODO start at some offset outside the barracks, not in the middle
                // ecb.SetComponent(spawn, LocalTransform.FromPosition(localTransform.ValueRO.Position + buildingState.ValueRO.RallyPositionOffset));
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
    }
}