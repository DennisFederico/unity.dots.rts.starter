using rts.components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;

namespace rts.systems {
    
    [UpdateInGroup(typeof(PhysicsSystemGroup))]
    public partial struct UnitMoveJobSystem : ISystem {
        
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

            state.Dependency = new UnitMoveJob {
                DeltaTime = SystemAPI.Time.DeltaTime,
                Ecb = ecb.AsParallelWriter(),
                StoppingDistance = 1f
            }.ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) {

        }
    }
    
    [BurstCompile]
    [WithAll(typeof(ShouldMove))]
    public partial struct UnitMoveJob : IJobEntity {
        
        public float DeltaTime;
        public EntityCommandBuffer.ParallelWriter Ecb;
        public float StoppingDistance;

        private void Execute(ref LocalTransform transform, in MoveData moveData, in MoveDestination destination, ref PhysicsVelocity physicsVelocity) {
            
            var direction = destination.Value - transform.Position;
            
            // TODO... IS IT WORTH IT TO PUT IN ANOTHER SYSTEM OR JOB?
            if (math.lengthsq(direction) < StoppingDistance) {
                physicsVelocity.Angular = float3.zero;
                physicsVelocity.Linear = float3.zero;
                // Ecb.SetComponentEnabled<ShouldMove>(index, entity, false);
                // Debug.Log($"Stopping unit {entity} at {transform.Position}");
                return;
            }
            
            direction = math.normalize(direction);
            
            var rotation = math.slerp(transform.Rotation,
                quaternion.LookRotationSafe(direction, math.up()),
                DeltaTime * moveData.RotationSpeed);
            
            transform.Rotation = rotation;
            physicsVelocity.Linear = direction * moveData.MoveSpeed;
            physicsVelocity.Angular = float3.zero;
        }
    }
}