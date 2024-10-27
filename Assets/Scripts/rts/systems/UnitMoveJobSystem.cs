using rts.components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;

namespace rts.systems {
    
    [UpdateInGroup(typeof(PhysicsSystemGroup))]
    public partial struct UnitMoveJobSystem : ISystem {

        private ComponentLookup<ShouldMove> _shouldMoveComponentLookup;
        
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            _shouldMoveComponentLookup = SystemAPI.GetComponentLookup<ShouldMove>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            _shouldMoveComponentLookup.Update(ref state);
            state.Dependency = new UnitMoveJob {
                DeltaTime = SystemAPI.Time.DeltaTime,
                Ecb = ecb.AsParallelWriter(),
                StoppingDistance = 0.1f,
                ShouldMoveLookup = _shouldMoveComponentLookup
            }.ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) {

        }
    }
    
    [BurstCompile]
    [WithPresent (typeof(ShouldMove))]
    public partial struct UnitMoveJob : IJobEntity {
        
        public float DeltaTime;
        public EntityCommandBuffer.ParallelWriter Ecb;
        public float StoppingDistance;
        [NativeDisableParallelForRestriction] public ComponentLookup<ShouldMove> ShouldMoveLookup;

        private void Execute(Entity entity, ref LocalTransform transform, in MoveData moveData, ref MoveDestination destination, ref PhysicsVelocity physicsVelocity) {
            
            var direction = destination.Value - transform.Position;
            
            // TODO... IS IT WORTH IT TO PUT IN ANOTHER SYSTEM OR JOB?
            if (math.lengthsq(direction) < StoppingDistance) {
                physicsVelocity.Angular = float3.zero;
                physicsVelocity.Linear = float3.zero;
                ShouldMoveLookup.SetComponentEnabled(entity, false);
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