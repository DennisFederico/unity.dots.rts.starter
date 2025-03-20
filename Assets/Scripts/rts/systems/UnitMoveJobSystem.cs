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

        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<PhysicsWorldSingleton>();
            state.RequireForUpdate<GridSystem.GridSystemData>();
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            shouldMoveComponentLookup = SystemAPI.GetComponentLookup<ShouldMove>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            var collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
            var gridSystemData = SystemAPI.GetSingleton<GridSystem.GridSystemData>();
            
            foreach (var (localTransform,
                         targetPositionQueued,
                         enabledTargetPositionQueued,
                         flowFieldPathRequest,
                         enabledFlowFieldPathRequest,
                         destination,
                         enabledShouldMove) in
                     SystemAPI.Query<
                             RefRO<LocalTransform>,
                             RefRW<TargetPositionQueued>,
                             EnabledRefRW<TargetPositionQueued>,
                             RefRW<FlowFieldPathRequest>,
                             EnabledRefRW<FlowFieldPathRequest>,
                             RefRW<MoveDestination>,
                             EnabledRefRW<ShouldMove>>()
                         .WithPresent<ShouldMove>()
                         .WithDisabled<FlowFieldPathRequest>()) {

                var raycastInput = new RaycastInput {
                    Start = localTransform.ValueRO.Position,
                    End = targetPositionQueued.ValueRO.Value,
                    Filter = new CollisionFilter {
                        BelongsTo = ~0u,
                        CollidesWith = GameConstants.PATHFINDING_HEAVY | GameConstants.PATHFINDING_OBSTACLES,
                        GroupIndex = 0
                    }
                };
                if (!collisionWorld.CastRay(raycastInput)) {
                    //No pathfinding needed
                    destination.ValueRW.Value = targetPositionQueued.ValueRO.Value;
                    enabledShouldMove.ValueRW = true;
                } else {
                    if (GridSystem.GridSystemData.IsWalkableGridPosition(targetPositionQueued.ValueRO.Value, gridSystemData)) {
                        // Do PathFind
                        flowFieldPathRequest.ValueRW.TargetPosition = targetPositionQueued.ValueRO.Value;
                        enabledFlowFieldPathRequest.ValueRW = true;    
                    } else {
                        destination.ValueRW.Value = localTransform.ValueRO.Position;
                        enabledShouldMove.ValueRW = true;
                    }
                }

                //Disable the target position queued whether it was a pathfinding or normal move
                enabledTargetPositionQueued.ValueRW = false;
            }

            
            foreach (var (localTransform,
                         follower,
                         enabledFollower,
                         destination,
                         enabledShouldMove) in
                     SystemAPI.Query<
                             RefRO<LocalTransform>,
                             RefRW<FlowFieldFollower>,
                             EnabledRefRW<FlowFieldFollower>,
                             RefRW<MoveDestination>,
                             EnabledRefRW<ShouldMove>>()
                         .WithPresent<ShouldMove>()) {
                //Current GridPosition
                var gridPosition = GridSystem.GridSystemData.GetGridPosition(localTransform.ValueRO.Position, gridSystemData.CellSize);
                //Get current grid cell vector
                var nodeIndex = GridSystem.GridSystemData.GetIndex(gridPosition, gridSystemData.XSize);
                var gridNodeEntity = gridSystemData.GridMapsArray[follower.ValueRO.FlowFieldIndex].GridNodes[nodeIndex];
                var gridNode = SystemAPI.GetComponent<GridSystem.GridNode>(gridNodeEntity);
                //Get the flow field vector
                var flowVector = GridSystem.GridSystemData.GetWorldFlowVector(gridNode.Vector);

                if (GridSystem.GridNode.IsWall(gridNode)) {
                    flowVector = follower.ValueRW.LastFlowFieldVector;
                } else {
                    follower.ValueRW.LastFlowFieldVector = flowVector;
                }

                if (math.distance(localTransform.ValueRO.Position, follower.ValueRO.TargetPosition) < gridSystemData.CellSize) {
                    enabledFollower.ValueRW = false;
                    destination.ValueRW.Value = localTransform.ValueRO.Position;
                } else {
                    var nodeWorldCenter = GridSystem.GridSystemData.GetWorldCenterPosition(gridPosition, gridSystemData.CellSize);
                    destination.ValueRW.Value = nodeWorldCenter + (flowVector * gridSystemData.CellSize * 1.5f);
                }

                var raycastInput = new RaycastInput {
                    Start = localTransform.ValueRO.Position,
                    End = follower.ValueRO.TargetPosition,
                    Filter = new CollisionFilter {
                        BelongsTo = ~0u,
                        CollidesWith = GameConstants.PATHFINDING_HEAVY | GameConstants.PATHFINDING_OBSTACLES,
                        GroupIndex = 0
                    }
                };

                if (!collisionWorld.CastRay(raycastInput)) {
                    //No pathfinding needed
                    Debug.Log("CLEAR SIGHT2");
                    destination.ValueRW.Value = follower.ValueRO.TargetPosition;
                    enabledFollower.ValueRW = false;
                }

                enabledShouldMove.ValueRW = true;
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
    [WithAll(typeof(ShouldMove))]
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