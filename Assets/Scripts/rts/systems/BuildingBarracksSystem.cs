using rts.authoring;
using Unity.Burst;
using Unity.Entities;

namespace rts.systems {
    public partial struct BuildingBarracksSystem : ISystem {
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {

            foreach (var buildingState in SystemAPI.Query<RefRW<BuildingBarracksState>>()) {
                buildingState.ValueRW.Progress += SystemAPI.Time.DeltaTime;

                if (buildingState.ValueRW.Progress < buildingState.ValueRW.ProgressRequired) continue;
                
                buildingState.ValueRW.Progress = 0;
            }
            
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
    }
}