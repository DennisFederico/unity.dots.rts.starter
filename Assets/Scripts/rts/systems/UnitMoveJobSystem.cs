using rts.authoring;
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
        private ComponentLookup<ShouldMove> shouldMoveComponentLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<GridSystem.GridSystemData>();
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            shouldMoveComponentLookup = SystemAPI.GetComponentLookup<ShouldMove>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            var gridSystemData = SystemAPI.GetSingleton<GridSystem.GridSystemData>();
            foreach (var (localTransform, follower, enabledFollower, destination) in
                     SystemAPI.Query<
                             RefRO<LocalTransform>,
                             RefRW<FlowFieldFollower>, 
                             EnabledRefRW<FlowFieldFollower>,
                             RefRW<MoveDestination>>()
                         .WithPresent<ShouldMove>()) {
                //Current GridPosition
                var gridPosition = GridSystem.GridSystemData.GetGridPosition(localTransform.ValueRO.Position, gridSystemData.CellSize);
                //Get current grid cell vector
                var nodeIndex = GridSystem.GridSystemData.GetIndex(gridPosition, gridSystemData.XSize);
                var gridNodeEntity = gridSystemData.GridMap.GridNodes[nodeIndex];
                var gridNode = SystemAPI.GetComponent<GridSystem.GridNode>(gridNodeEntity);
                //Get the flow field vector
                var flowVector = GridSystem.GridSystemData.GetWorldFlowVector(gridNode.Vector);
                
                if (GridSystem.GridNode.IsWall(gridNode)) {
                    flowVector = follower.ValueRW.LastFlowFieldVector;
                } else {
                    follower.ValueRW.LastFlowFieldVector = flowVector;
                }

                if (math.distance(localTransform.ValueRO.Position, follower.ValueRO.TargetPosition) < gridSystemData.CellSize *.5f) {
                    enabledFollower.ValueRW = false;
                    destination.ValueRW.Value = localTransform.ValueRO.Position;
                } else {
                    var nodeWorldCenter = GridSystem.GridSystemData.GetWorldCenterPosition(gridPosition, gridSystemData.CellSize);
                    destination.ValueRW.Value = nodeWorldCenter + (flowVector * gridSystemData.CellSize * 2f);    
                }
            }

            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            shouldMoveComponentLookup.Update(ref state);
            state.Dependency = new UnitMoveJob {
                DeltaTime = SystemAPI.Time.DeltaTime,
                Ecb = ecb.AsParallelWriter(),
                StoppingDistance = 0.5f,
                ShouldMoveLookup = shouldMoveComponentLookup
            }.ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
    }

    [BurstCompile]
    [WithPresent(typeof(ShouldMove))]
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