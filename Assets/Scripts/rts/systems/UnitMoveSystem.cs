using rts.components;
using rts.mono;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace rts.systems {
    
    [DisableAutoCreation]
    public partial struct UnitMoveSystem : ISystem {
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            foreach (var (transform, moveData, destination, physicsVelocity) in 
                     SystemAPI.Query<RefRW<LocalTransform>, RefRO<MoveData>, RefRO<MoveDestination>, RefRW<PhysicsVelocity>>()
                         .WithAll<ShouldMove>()) {
                //TODO USING A MANAGED INSTANCE IS NOT BURSTABLE
                float3 targetPos = destination.ValueRO.Value;
                float3 dir = math.normalize(targetPos - transform.ValueRO.Position);
                
                var rotation = math.slerp(transform.ValueRO.Rotation,
                    quaternion.LookRotationSafe(dir, math.up()),
                    SystemAPI.Time.DeltaTime * moveData.ValueRO.RotationSpeed);
                
                transform.ValueRW.Rotation = rotation;
                physicsVelocity.ValueRW.Linear = dir * moveData.ValueRO.MoveSpeed;
                physicsVelocity.ValueRW.Angular = float3.zero;
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) {

        }
    }
}