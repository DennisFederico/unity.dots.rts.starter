using rts.components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace rts.systems {

    [UpdateInGroup(typeof(TargetingSystemGroup))]
    public partial struct FindTargetSystem : ISystem {

        private int _tick;

        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<PhysicsWorldSingleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            var collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;

            foreach (var (targetingData, localTransform, attackTarget) in
                     SystemAPI.Query<RefRO<TargetingData>, RefRO<LocalTransform>, RefRW<Target>>()) {
                
                var collisionFilter = new CollisionFilter {
                    BelongsTo = ~0u,
                    CollidesWith = (uint) targetingData.ValueRO.TargetLayers.value,
                    GroupIndex = 0
                };
                
                var closestHitCollector = new ClosestHitCollector<DistanceHit>(targetingData.ValueRO.Range);
                if (collisionWorld.OverlapSphereCustom(localTransform.ValueRO.Position, targetingData.ValueRO.Range, ref closestHitCollector, collisionFilter) && 
                    SystemAPI.HasComponent<LocalTransform>(closestHitCollector.ClosestHit.Entity)) {
                    
                    attackTarget.ValueRW.Value = closestHitCollector.ClosestHit.Entity;
                    var offset =  new float3(0,0,1.5f);
                    if (SystemAPI.HasComponent<AttackTargetOffset>(closestHitCollector.ClosestHit.Entity)) {
                        offset = SystemAPI.GetComponent<AttackTargetOffset>(closestHitCollector.ClosestHit.Entity).Value;
                    }
                    attackTarget.ValueRW.AttackOffset = offset;
                } else {
                    attackTarget.ValueRW.Value = Entity.Null;
                    attackTarget.ValueRW.AttackOffset = float3.zero;
                }
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) {
        }
    }
}