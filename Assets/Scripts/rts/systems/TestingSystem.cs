using rts.components;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace rts.systems {
    
    [DisableAutoCreation]
    public partial struct TestingSystem : ISystem {

        private EntityQuery _query;
        
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            _query = SystemAPI.QueryBuilder()
                // .WithAll<EnemyTag>()
                .WithAll<TargetingData, LocalTransform, Target>()
                .Build();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            state.Enabled = false;
            Debug.Log($"There are {_query.CalculateEntityCount()} enemies");
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) {

        }
    }
}