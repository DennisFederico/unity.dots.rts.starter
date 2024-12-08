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
            var defaultOffset =  new float3(0,0,1.5f);
            
            foreach (var (targetingData, 
                         localTransform, 
                         target, 
                         targetOverride) in
                     SystemAPI.Query<RefRO<TargetingData>, RefRO<LocalTransform>, RefRW<Target>, RefRO<TargetOverride>>()) {
                
                if (targetOverride.ValueRO.Value != Entity.Null) {
                    target.ValueRW.Value = targetOverride.ValueRO.Value;
                    target.ValueRW.AttackOffset = targetOverride.ValueRO.AttackOffset;
                    continue;
                }
                
                var collisionFilter = new CollisionFilter {
                    BelongsTo = ~0u,
                    CollidesWith = (uint) targetingData.ValueRO.TargetLayers.value,
                    GroupIndex = 0
                };

                var closestHitCollector = new ClosestHitCollector<DistanceHit>(targetingData.ValueRO.Range);
                if (collisionWorld.OverlapSphereCustom(localTransform.ValueRO.Position, targetingData.ValueRO.Range, ref closestHitCollector, collisionFilter) && 
                    SystemAPI.HasComponent<LocalTransform>(closestHitCollector.ClosestHit.Entity)) { 
                    
                    target.ValueRW.Value = closestHitCollector.ClosestHit.Entity;
                    var offset =  SystemAPI.HasComponent<AttackTargetOffset>(closestHitCollector.ClosestHit.Entity) ? 
                        SystemAPI.GetComponent<AttackTargetOffset>(closestHitCollector.ClosestHit.Entity).Value : defaultOffset;
                    target.ValueRW.AttackOffset = offset;
                } else {
                    target.ValueRW.Value = Entity.Null;
                    target.ValueRW.AttackOffset = float3.zero;
                }
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) {
        }
    }
}