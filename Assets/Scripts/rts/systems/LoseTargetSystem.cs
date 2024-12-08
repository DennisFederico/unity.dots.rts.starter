using rts.components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace rts.systems {
    
    [UpdateInGroup(typeof(TargetingSystemGroup))]
    [UpdateAfter(typeof(FindTargetSystem))]
    public partial struct LoseTargetSystem : ISystem {
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<LoseTargetDistance>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            foreach (var (localTransform, 
                         loseDistance, 
                         target, 
                         targetOverride) in
                     SystemAPI.Query<RefRO<LocalTransform>, RefRO<LoseTargetDistance>, RefRW<Target>, RefRO<TargetOverride>>()) {
                
                if (target.ValueRO.Value == Entity.Null || !SystemAPI.HasComponent<LocalTransform>(target.ValueRO.Value)) continue;
                if (targetOverride.ValueRO.Value != Entity.Null) continue;
                var distance = math.distance(localTransform.ValueRO.Position, SystemAPI.GetComponent<LocalTransform>(target.ValueRO.Value).Position);
                if (distance < loseDistance.ValueRO.Value) continue;
                target.ValueRW.Value = Entity.Null;
                target.ValueRW.AttackOffset = float3.zero;
                //TODO should we loose also the Target Override??
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) {
        }
    }
}