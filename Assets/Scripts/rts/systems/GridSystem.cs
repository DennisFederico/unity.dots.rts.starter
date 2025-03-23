//#define GRID_DEBUG
using rts.authoring;
using rts.mono;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;

namespace rts.systems {
    public partial struct GridSystem : ISystem {
        public const int FLOW_FIELD_MAPS_COUNT = 32;
        private ComponentLookup<GridNode> gridNodeComponentLookup;

        public struct GridSystemData : IComponentData {
            public int XSize;
            public int YSize;
            public float CellSize;
            public NativeArray<GridMap> GridMapsArray;
            public int NextGridIndex;
            public NativeArray<byte> CostMap;
            public NativeArray<Entity> AllGridMapEntities;

            public const byte WALL_COST = byte.MaxValue;
            public const byte HEAVY_COST = 250;

            public static int GetIndex(int posX, int posY, int xSize) {
                return posX + posY * xSize;
            }

            public static int GetIndex(int2 gridPosition, int xSize) {
                return gridPosition.x + gridPosition.y * xSize;
            }

            public static int2 GetGridPosition(int index, int xSize) {
                return new int2(index % xSize, index / xSize);
            }

            public static int2 GetGridPosition(float3 worldPosition, float cellSize) {
                return new int2(
                    (int)math.floor(worldPosition.x / cellSize),
                    (int)math.floor(worldPosition.z / cellSize));
            }

            public static float3 GetWorldPosition(int2 gridPosition, float cellSize) {
                return new float3(gridPosition.x * cellSize, 0f, gridPosition.y * cellSize);
            }

            public static float3 GetWorldCenterPosition(int xPos, int yPos, float cellSize) {
                return new float3(
                    xPos * cellSize + cellSize * .5f,
                    0f,
                    yPos * cellSize + cellSize * .5f);
            }

            public static float3 GetWorldCenterPosition(int2 gridPosition, float cellSize) {
                return new float3(
                    gridPosition.x * cellSize + cellSize * .5f,
                    0f,
                    gridPosition.y * cellSize + cellSize * .5f);
            }

            public static float3 GetWorldFlowVector(float2 flowFieldVector) {
                return new float3(flowFieldVector.x, 0f, flowFieldVector.y);
            }

            public bool IsWithinBounds(int2 index) {
                return index.x >= 0 && index.x < XSize && index.y >= 0 && index.y < YSize;
            }
            
            public static bool IsWithinBounds(int2 index, int xSize, int ySize) {
                return index.x >= 0 && index.x < xSize && index.y >= 0 && index.y < ySize;
            }

            public static bool IsWalkableGridPosition(float3 worldPosition, int xSize, int ySize, float cellSize, NativeArray<byte> costMap) {
                var gridPosition = GridSystemData.GetGridPosition(worldPosition, cellSize);
                return IsWithinBounds(gridPosition, xSize, ySize) && !GridNode.IsWall(gridPosition, xSize, costMap);
            }
        }

        public struct GridMap {
            //TODO ADD A SHARED COMPONENT TO HAVE ALL THE ENTITIES IN THE SAME CHUNK AND ALLOW FASTER QUERIES
            public NativeArray<Entity> GridNodes;
            public int2 GridPositionTarget;
            public bool IsValid;
        }

        public struct GridNode : IComponentData {
            public int GridMapIndex;
            public int GridNodeIndex;
            public int2 GridNodePosition;
            public byte Cost;
            public int BestCosts;
            public int2 Vector;

            public static bool IsWall(GridNode gridNode) {
                return gridNode.Cost == GridSystemData.WALL_COST;
            }

            public static bool IsWall(int2 gridPosition, int xSize, NativeArray<byte> costMap) {
                var index = GridSystemData.GetIndex(gridPosition, xSize);
                return costMap[index] == GridSystemData.WALL_COST;
            }
        }

        private float elapsedTime;

        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<PhysicsWorldSingleton>();

            gridNodeComponentLookup = SystemAPI.GetComponentLookup<GridNode>();

            int xSize = 20;
            int ySize = 20;
            int totalSize = xSize * ySize;
            float cellSize = 5f;

            //GridNode entity Template
            var gridNodeTemplate = state.EntityManager.CreateEntity();
            state.EntityManager.AddComponent<GridNode>(gridNodeTemplate);

            //Allocate GridMaps array
            var gridMapsArray = new NativeArray<GridMap>(FLOW_FIELD_MAPS_COUNT, Allocator.Persistent);
            //HACK TO HAVE ALL ENTITIES IN ONE PLACE TO AVOID NESTED CONTAINERS
            var allGridMapEntitiesList = new NativeList<Entity>(FLOW_FIELD_MAPS_COUNT * totalSize, Allocator.Temp);

            for (int i = 0; i < FLOW_FIELD_MAPS_COUNT; i++) {
                //GridNodes array
                var gridNodes = state.EntityManager.Instantiate(gridNodeTemplate, totalSize, Allocator.Persistent);
                allGridMapEntitiesList.AddRange(gridNodes);

                for (int y = 0; y < ySize; y++) {
                    for (int x = 0; x < xSize; x++) {
                        var index = GridSystemData.GetIndex(x, y, xSize);
                        var gridNode = new GridNode() {
                            GridMapIndex = i,
                            GridNodeIndex = index,
                            GridNodePosition = new int2(x, y)
                        };
                        state.EntityManager.SetComponentData(gridNodes[index], gridNode);
                        state.EntityManager.SetName(gridNodes[index], $"GN_M{i}_[{x}-{y}]");
                    }
                }

                //Create GridMap
                var gridMap = new GridMap() {
                    GridNodes = gridNodes,
                    IsValid = false
                };

                gridMapsArray[i] = gridMap;
            }

            //Set the GridSystemData component
            state.EntityManager.AddComponentData(state.SystemHandle, new GridSystemData() {
                XSize = xSize,
                YSize = ySize,
                CellSize = cellSize,
                GridMapsArray = gridMapsArray,
                CostMap = new NativeArray<byte>(totalSize, Allocator.Persistent),
                NextGridIndex = 0,
                AllGridMapEntities = allGridMapEntitiesList.ToArray(Allocator.Persistent)
            });
            
            allGridMapEntitiesList.Dispose();
        }

#if !GRID_DEBUG
        [BurstCompile]
#endif
        public void OnUpdate(ref SystemState state) {
            var gridSystemData = SystemAPI.GetComponent<GridSystemData>(state.SystemHandle);
#if GRID_DEBUG
            bool updateDebugGrid = false;
            int gridMapIndexToDebug = -1;
#endif
            // int gridMapIndex;
            foreach (var (request,
                         enableRequest,
                         follower,
                         enableFollower) in
                     SystemAPI.Query<
                         RefRW<FlowFieldPathRequest>,
                         EnabledRefRW<FlowFieldPathRequest>,
                         RefRW<FlowFieldFollower>,
                         EnabledRefRW<FlowFieldFollower>
                     >().WithPresent<FlowFieldFollower>().WithDisabled<TargetPositionQueued>()) {
                var targetPosition = GridSystemData.GetGridPosition(request.ValueRO.TargetPosition, gridSystemData.CellSize);

                if (gridSystemData.IsWithinBounds(targetPosition)) {
                    //TODO Use a hashmap or similar to track indexes and target Position to avoid the loop
                    if (ExistingFlowFieldForTarget(ref gridSystemData, targetPosition, out var gridMapIndex)) {
                        enableRequest.ValueRW = false;
                        enableFollower.ValueRW = true;
                        follower.ValueRW.TargetPosition = request.ValueRO.TargetPosition;
                        follower.ValueRW.FlowFieldIndex = gridMapIndex;
#if GRID_DEBUG
                        updateDebugGrid = true;
                        gridMapIndexToDebug = gridMapIndex;
#endif
                    } else {
                        gridMapIndex = gridSystemData.NextGridIndex;
                        gridSystemData.NextGridIndex = (gridSystemData.NextGridIndex + 1) % FLOW_FIELD_MAPS_COUNT;
                        SystemAPI.SetComponent(state.SystemHandle, gridSystemData);

                        InitializeGrid(ref state, gridMapIndex, targetPosition);
                        PlaceGridObstacles(ref state, gridSystemData, gridMapIndex);

                        var gridNodes = GetGridNodes_TEMP(ref state, gridSystemData, gridMapIndex);

                        GenerateFlowField(gridSystemData, targetPosition, gridNodes);
                        
                        var gridMap = gridSystemData.GridMapsArray[gridMapIndex];
                        gridMap.GridPositionTarget = targetPosition;
                        gridMap.IsValid = true;
                        gridSystemData.GridMapsArray[gridMapIndex] = gridMap;
                        SystemAPI.SetComponent(state.SystemHandle, gridSystemData);

                        gridNodes.Dispose();

                        enableRequest.ValueRW = false;
                        enableFollower.ValueRW = true;
                        follower.ValueRW.TargetPosition = request.ValueRO.TargetPosition;
                        follower.ValueRW.FlowFieldIndex = gridMapIndex;
#if GRID_DEBUG
                        updateDebugGrid = true;
                        gridMapIndexToDebug = gridMapIndex;
#endif
                    }
                }
            }
#if GRID_DEBUG
            if (updateDebugGrid) GridSystemDebug.Instance.UpdateGridDebug(gridSystemData, gridMapIndexToDebug);
#endif
        }

        [BurstCompile]
        private bool ExistingFlowFieldForTarget(ref GridSystemData gridSystemData, int2 targetPosition, out int gridIndex) {
            for (int i = 0; i < FLOW_FIELD_MAPS_COUNT; i++) {
                if (gridSystemData.GridMapsArray[i].IsValid && gridSystemData.GridMapsArray[i].GridPositionTarget.Equals(targetPosition)) {
                    gridIndex = i;
                    return true;
                }
            }

            gridIndex = -1;
            return false;
        }

        [BurstCompile]
        private void InitializeGrid(ref SystemState state, int gridMapIndex, int2 targetPosition) {
            var initializeGridHandle = new InitializeGridJob() {
                GridMapIndex = gridMapIndex,
                TargetGridPosition = targetPosition
            }.ScheduleParallel(state.Dependency);
            initializeGridHandle.Complete();
        }

        [BurstCompile]
        private NativeArray<RefRW<GridNode>> GetGridNodes_TEMP(ref SystemState state, GridSystemData gridSystemData, int gridMapIndex) {
            var gridNodes = new NativeArray<RefRW<GridNode>>(gridSystemData.XSize * gridSystemData.YSize, Allocator.Temp);
            for (int y = 0; y < gridSystemData.YSize; y++) {
                for (int x = 0; x < gridSystemData.XSize; x++) {
                    var index = GridSystemData.GetIndex(x, y, gridSystemData.XSize);
                    var gridNode = SystemAPI.GetComponentRW<GridNode>(gridSystemData.GridMapsArray[gridMapIndex].GridNodes[index]);
                    gridNodes[index] = gridNode;
                }
            }

            return gridNodes;
        }

        [BurstCompile]
        private void PlaceGridObstacles(ref SystemState state, GridSystemData gridSystemData, int gridMapIndex) {
            //Take the entities colliders and mark the grid nodes the span as obstacles
            var pws = SystemAPI.GetSingleton<PhysicsWorldSingleton>();

            gridNodeComponentLookup.Update(ref state);

            new UpdateCostMapJob() {
                CollisionWorld = pws.CollisionWorld,
                CollisionFilterWalls = new CollisionFilter() {
                    BelongsTo = ~0u,
                    CollidesWith = GameConstants.PATHFINDING_OBSTACLES,
                    GroupIndex = 0
                },
                CollisionFilterHeavy = new CollisionFilter() {
                    BelongsTo = ~0u,
                    CollidesWith = GameConstants.PATHFINDING_HEAVY,
                    GroupIndex = 0
                },
                CostMap = gridSystemData.CostMap,
                GridMap = gridSystemData.GridMapsArray[gridMapIndex],
                GridNodeLookup = gridNodeComponentLookup,
                XSize = gridSystemData.XSize,
                CellSize = gridSystemData.CellSize
            }.Schedule(gridSystemData.XSize * gridSystemData.YSize, 50).Complete();
        }

        //This defines the orders on which cells are evaluated.
        private static readonly int2[] Neighbours = {
            new(0, 1),
            new(1, 0),
            new(0, -1),
            new(-1, 0),
            new(1, 1),
            new(1, -1),
            new(-1, 1),
            new(-1, -1)
        };

        [BurstCompile]
        private void GenerateFlowField(GridSystemData gridSystemData, int2 startingGridPosition, NativeArray<RefRW<GridNode>> gridNodes) {
            var openQueue = new NativeQueue<int2>(Allocator.Temp);
            openQueue.Enqueue(startingGridPosition);

            int safeLimit = (int)(gridSystemData.XSize * gridSystemData.YSize * 1.1f);
            int iterations = 0;

            while (openQueue.Count > 0) {
                iterations++;
                if (iterations > safeLimit) {
                    Debug.LogError("Safe limit reached");
                    break;
                }

                //Dequeue position
                var currentGridPosition = openQueue.Dequeue();
                var currentGridNode = gridNodes[GridSystemData.GetIndex(currentGridPosition, gridSystemData.XSize)];

                //Visit the neighbours
                foreach (int2 neighbour in Neighbours) {
                    if (neighbour is { x: 0, y: 0 }) continue;
                    var neighbourGridPosition = currentGridPosition + neighbour;
                    if (!gridSystemData.IsWithinBounds(neighbourGridPosition)) continue;

                    var neighbourGridIndex = GridSystemData.GetIndex(neighbourGridPosition, gridSystemData.XSize);
                    var neighbourGridNode = gridNodes[neighbourGridIndex];

                    if (neighbourGridNode.ValueRO.Cost == GridSystemData.WALL_COST) continue;

                    var newCost = currentGridNode.ValueRO.BestCosts + neighbourGridNode.ValueRO.Cost;
                    if (newCost < neighbourGridNode.ValueRO.BestCosts) {
                        neighbourGridNode.ValueRW.BestCosts = newCost;
                        neighbourGridNode.ValueRW.Vector = currentGridPosition - neighbourGridPosition;
                        openQueue.Enqueue(neighbourGridPosition);
                    }
                }
            }
            openQueue.Dispose();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) {
            var gridSystemData = SystemAPI.GetComponentRW<GridSystemData>(state.SystemHandle);

            //Destroy each GridMap of the GridMaps array
            for (var i = 0; i < FLOW_FIELD_MAPS_COUNT; i++) {
                //state.EntityManager.DestroyEntity(gridSystemData.ValueRW.GridMapsArray[i].GridNodes);
                gridSystemData.ValueRW.GridMapsArray[i].GridNodes.Dispose();
            }

            gridSystemData.ValueRW.GridMapsArray.Dispose();
            gridSystemData.ValueRW.CostMap.Dispose();
            gridSystemData.ValueRW.AllGridMapEntities.Dispose();
        }

        [BurstCompile]
        public partial struct InitializeGridJob : IJobEntity {
            [ReadOnly] public int GridMapIndex;
            [ReadOnly] public int2 TargetGridPosition;

            public void Execute(ref GridNode gridNode) {
                
                //TODO This check is absurd, we should be able to use a shared component with the value of the gridMapIndex and use that to filter the entities
                //Such that all GridNode entities for a given GridMap are in the same chunk
                //From the Update method use a Query and then schedule the jub using it...
                // _query = SystemAPI.QueryBuilder()....Build();
                // var job = new UpdateJob();
                // state.Dependency = job.ScheduleParallel(_query, state.Dependency);
                if (gridNode.GridMapIndex != GridMapIndex) return;
                if (gridNode.GridNodePosition.Equals(TargetGridPosition)) {
                    gridNode.Cost = 0;
                    gridNode.BestCosts = 0;
                } else {
                    gridNode.Cost = 1;
                    gridNode.BestCosts = int.MaxValue;
                }

                //If stuck, it will eventually move to a valid node
                gridNode.Vector = new int2(0, 0);
            }
        }

        [BurstCompile]
        public struct UpdateCostMapJob : IJobParallelFor {
            
            [NativeDisableParallelForRestriction] public ComponentLookup<GridNode> GridNodeLookup;
            [NativeDisableParallelForRestriction] public NativeArray<byte> CostMap;
            
            [ReadOnly] public CollisionWorld CollisionWorld;
            [ReadOnly] public CollisionFilter CollisionFilterWalls;
            [ReadOnly] public CollisionFilter CollisionFilterHeavy;
            [ReadOnly] public GridMap GridMap;
            [ReadOnly] public int XSize;
            [ReadOnly] public float CellSize;

            public void Execute(int index) {
                var halfCellSize = CellSize * .5f;
                var gridPosition = GridSystemData.GetGridPosition(index, XSize);
                var worldPosition = GridSystemData.GetWorldCenterPosition(gridPosition.x, gridPosition.y, CellSize);
                NativeList<DistanceHit> distanceHits = new NativeList<DistanceHit>(Allocator.TempJob);

                //TODO Confirm which method is faster, a full AABB collider overlap for the size of the grid or going through the grid nodes and using a sphere/box collider for each one
                //Method 1. Iterating through the grid nodes using sphere collider

                if (CollisionWorld.OverlapSphere(worldPosition, halfCellSize, ref distanceHits, CollisionFilterWalls)) {
                    // var index = GridSystemData.GetIndex(x, y, XSize);
                    var gridNode = GridNodeLookup[GridMap.GridNodes[index]];
                    gridNode.Cost = GridSystemData.WALL_COST;
                    GridNodeLookup[GridMap.GridNodes[index]] = gridNode;
                    // costMap[index] = GridSystemData.WALL_COST;
                    CostMap[index] = GridSystemData.WALL_COST;
                }

                distanceHits.Clear();

                //TODO Confirm which method is faster, a full AABB collider overlap for the size of the grid or going through the grid nodes and using a sphere/box collider for each one
                //Method 1. Iterating through the grid nodes using sphere collider
                if (CollisionWorld.OverlapSphere(worldPosition, halfCellSize, ref distanceHits, CollisionFilterHeavy)) {
                    var gridNode = GridNodeLookup[GridMap.GridNodes[index]];
                    gridNode.Cost = GridSystemData.HEAVY_COST;
                    GridNodeLookup[GridMap.GridNodes[index]] = gridNode;
                    // costMap[index] = GridSystemData.HEAVY_COST;
                    CostMap[index] = GridSystemData.HEAVY_COST;
                }

                distanceHits.Dispose();
            }
        }
    }
}