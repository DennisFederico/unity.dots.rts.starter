using rts.authoring;
using rts.components;
using rts.mono;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;

namespace rts.systems {
    [UpdateInGroup(typeof(PhysicsSystemGroup))]
    public partial struct UnitMoveJobSystem : ISystem {
        private ComponentLookup<ShouldMove> shouldMoveComponentLookup;
        private ComponentLookup<GridSystem.GridNode> gridNodeComponentLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<PhysicsWorldSingleton>();
            state.RequireForUpdate<GridSystem.GridSystemData>();
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            shouldMoveComponentLookup = SystemAPI.GetComponentLookup<ShouldMove>();
            gridNodeComponentLookup = SystemAPI.GetComponentLookup<GridSystem.GridNode>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            var collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
            var gridSystemData = SystemAPI.GetSingleton<GridSystem.GridSystemData>();
            
            // new TargetPositionQueuedJob() {
            //     CollisionWorld = collisionWorld,
            //     CellSize = gridSystemData.CellSize,
            //     XSize = gridSystemData.XSize,
            //     YSize = gridSystemData.YSize,
            //     CostMap = gridSystemData.CostMap
            // }.ScheduleParallel();
            //
            // new CanMoveStraightJob() {
            //     CollisionWorld = collisionWorld,
            //     ObstaclesFilter = new CollisionFilter {
            //         BelongsTo = ~0u,
            //         CollidesWith = GameConstants.PATHFINDING_HEAVY | GameConstants.PATHFINDING_OBSTACLES,
            //         GroupIndex = 0
            //     }
            // }.ScheduleParallel();
            //
            // gridNodeComponentLookup.Update(ref state);
            // new FlowFieldFollowerJob() {
            //     GridNodeLookup = gridNodeComponentLookup,
            //     AllGridMapsEntities = gridSystemData.AllGridMapEntities,
            //     CellSize = gridSystemData.CellSize,
            //     XSize = gridSystemData.XSize,
            //     NumEntitiesPerGrid = gridSystemData.XSize * gridSystemData.YSize
            // }.ScheduleParallel();

            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            shouldMoveComponentLookup.Update(ref state);
            new UnitMoveJob {
                DeltaTime = SystemAPI.Time.DeltaTime,
                Ecb = ecb.AsParallelWriter(),
                StoppingDistance = 2f,
                // ShouldMoveLookup = shouldMoveComponentLookup
            }.ScheduleParallel();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
    }

    [BurstCompile]
    // [WithPresent(typeof(ShouldMove))]
    [WithAll(typeof(ShouldMove))]
    [WithDisabled(typeof(FlowFieldPathRequest))]
    public partial struct TargetPositionQueuedJob : IJobEntity {
        [ReadOnly] public CollisionWorld CollisionWorld;
        [ReadOnly] public float CellSize;
        [ReadOnly] public int XSize;
        [ReadOnly] public int YSize;
        [ReadOnly] public NativeArray<byte> CostMap;

        private void Execute(
            in LocalTransform localTransform,
            ref TargetPositionQueued targetPositionQueued,
            EnabledRefRW<TargetPositionQueued> enabledTargetPositionQueued,
            ref FlowFieldPathRequest flowFieldPathRequest,
            EnabledRefRW<FlowFieldPathRequest> enabledFlowFieldPathRequest,
            ref MoveDestination destination,
            EnabledRefRW<ShouldMove> enabledShouldMove) {
            var raycastInput = new RaycastInput {
                Start = localTransform.Position,
                End = targetPositionQueued.Value,
                Filter = new CollisionFilter {
                    BelongsTo = ~0u,
                    CollidesWith = GameConstants.PATHFINDING_HEAVY | GameConstants.PATHFINDING_OBSTACLES,
                    GroupIndex = 0
                }
            };
            if (!CollisionWorld.CastRay(raycastInput)) {
                //No pathfinding needed
                destination.Value = targetPositionQueued.Value;
                enabledShouldMove.ValueRW = true;
            } else {
                if (GridSystem.GridSystemData.IsWalkableGridPosition(targetPositionQueued.Value, XSize, YSize, CellSize, CostMap)) {
                    // Do PathFind
                    flowFieldPathRequest.TargetPosition = targetPositionQueued.Value;
                    enabledFlowFieldPathRequest.ValueRW = true;
                } else {
                    destination.Value = localTransform.Position;
                    enabledShouldMove.ValueRW = true;
                }
            }

            //Disable the target position queued whether it was a pathfinding or normal move
            enabledTargetPositionQueued.ValueRW = false;
        }
    }

    [BurstCompile]
    [WithAll(typeof(FlowFieldFollower), typeof(MoveDestination), typeof(ShouldMove))]
    // [WithAll(typeof(FlowFieldFollower), typeof(MoveDestination))]
    // [WithPresent(typeof(ShouldMove))]
    public partial struct CanMoveStraightJob : IJobEntity {
        [ReadOnly] public CollisionWorld CollisionWorld;
        [ReadOnly] public CollisionFilter ObstaclesFilter;

        private void Execute(in LocalTransform localTransform, ref MoveDestination destination, ref FlowFieldFollower follower, EnabledRefRW<FlowFieldFollower> enabledFollower, EnabledRefRW<ShouldMove> enabledShouldMove) {
            var raycastInput = new RaycastInput {
                Start = localTransform.Position,
                End = follower.TargetPosition,
                Filter = ObstaclesFilter
            };

            if (!CollisionWorld.CastRay(raycastInput)) {
                //No pathfinding needed
                destination.Value = follower.TargetPosition;
                enabledFollower.ValueRW = false;
                enabledShouldMove.ValueRW = true;
            }
        }
    }
    
    [BurstCompile]
    // [WithAll(typeof(FlowFieldFollower), typeof(MoveDestination))]
    [WithAll(typeof(FlowFieldFollower), typeof(MoveDestination), typeof(ShouldMove))]
    // [WithPresent(typeof(ShouldMove))]
    public partial struct FlowFieldFollowerJob : IJobEntity {
        
        [ReadOnly] public ComponentLookup<GridSystem.GridNode> GridNodeLookup;
        [ReadOnly] public NativeArray<Entity> AllGridMapsEntities;
        [ReadOnly] public float CellSize;
        [ReadOnly] public int XSize;
        [ReadOnly] public int NumEntitiesPerGrid;
        
        private void Execute(
            in LocalTransform localTransform,
            ref FlowFieldFollower follower,
            EnabledRefRW<FlowFieldFollower> enabledFollower,
            ref MoveDestination destination,
            EnabledRefRW<ShouldMove> enabledShouldMove) {
            
            //Current GridPosition
            var gridPosition = GridSystem.GridSystemData.GetGridPosition(localTransform.Position, CellSize);
            //Get current grid cell vector
            var nodeIndex = GridSystem.GridSystemData.GetIndex(gridPosition, XSize);
            // var gridNodeEntity = GridSystemData.GridMapsArray[follower.FlowFieldIndex].GridNodes[nodeIndex];
            var gridNodeEntity = AllGridMapsEntities[NumEntitiesPerGrid * follower.FlowFieldIndex + nodeIndex];
            var gridNode = GridNodeLookup[gridNodeEntity];
            //Get the flow field vector
            var flowVector = GridSystem.GridSystemData.GetWorldFlowVector(gridNode.Vector);

            if (GridSystem.GridNode.IsWall(gridNode)) {
                flowVector = follower.LastFlowFieldVector;
            } else {
                follower.LastFlowFieldVector = flowVector;
            }

            if (math.distance(localTransform.Position, follower.TargetPosition) < CellSize) {
                enabledFollower.ValueRW = false;
                destination.Value = localTransform.Position;
            } else {
                var nodeWorldCenter = GridSystem.GridSystemData.GetWorldCenterPosition(gridPosition, CellSize);
                destination.Value = nodeWorldCenter + (flowVector * CellSize * 1.5f);
            }

            enabledShouldMove.ValueRW = true;
        }
    }
    
    [BurstCompile]
    [WithPresent(typeof(ShouldMove))]
    public partial struct UnitMoveJob : IJobEntity {
        public float DeltaTime;
        public EntityCommandBuffer.ParallelWriter Ecb;
        public float StoppingDistance;
        // [NativeDisableParallelForRestriction] public ComponentLookup<ShouldMove> ShouldMoveLookup;

        private void Execute(Entity entity, ref LocalTransform transform, in MoveData moveData, ref MoveDestination destination, ref PhysicsVelocity physicsVelocity, EnabledRefRW<ShouldMove> enabledShouldMove) {
            Debug.Log($"UnitMoveJob - Entity: {entity.Index} - ShouldMove: {enabledShouldMove.ValueRO}");
            var direction = destination.Value - transform.Position;

            // TODO... IS IT WORTH IT TO PUT IN ANOTHER SYSTEM OR JOB?
            if (math.lengthsq(direction) < StoppingDistance) {
                Debug.Log($"Stopping distance reached, stopping movement {enabledShouldMove.ValueRO}");
                physicsVelocity.Angular = float3.zero;
                physicsVelocity.Linear = float3.zero;
                destination.Value = transform.Position;
                enabledShouldMove.ValueRW = false;
                // ShouldMoveLookup.SetComponentEnabled(entity, false);
                return;
            }
            Debug.Log($"Moving towards {destination.Value}, distance: {math.distance(transform.Position, destination.Value)}");

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