#define GRID_DEBUG
using rts.authoring;
using rts.mono;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;

namespace rts.systems {
    public partial struct GridSystem : ISystem {
        
        public struct GridSystemData : IComponentData {
            public int XSize;
            public int YSize;
            public float CellSize;
            public GridMap GridMap;
            
            public const byte WALL_COST = byte.MaxValue;

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

            public bool IsWithinBounds(int x, int y) {
                return x >= 0 && x < XSize && y >= 0 && y < YSize;
            }

            public bool IsWithinBounds(int2 index) {
                return index.x >= 0 && index.x < XSize && index.y >= 0 && index.y < YSize;
            }
        }

        public struct GridMap {
            public NativeArray<Entity> GridNodes;
        }

        public struct GridNode : IComponentData {
            public int Index;
            public int2 GridPosition;
            public byte Cost;
            public byte BestCosts;
            public int2 Vector;

            public static bool IsWall(GridNode gridNode) {
                return gridNode.Cost == GridSystemData.WALL_COST;
            }
        }

        private float elapsedTime;

        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<PhysicsWorldSingleton>();
            int xSize = 20;
            int ySize = 10;
            int totalSize = xSize * ySize;
            float cellSize = 5f;

            //GridNode entity Template
            var gridNodeTemplate = state.EntityManager.CreateEntity();
            state.EntityManager.AddComponent<GridNode>(gridNodeTemplate);

            //GridNodes array
            var gridNodes = state.EntityManager.Instantiate(gridNodeTemplate, totalSize, Allocator.Persistent);

            for (int y = 0; y < ySize; y++) {
                for (int x = 0; x < xSize; x++) {
                    var index = GridSystemData.GetIndex(x, y, xSize);
                    var gridNode = new GridNode() {
                        Index = index,
                        GridPosition = new int2(x, y)
                    };
                    state.EntityManager.SetComponentData(gridNodes[index], gridNode);
                    state.EntityManager.SetName(gridNodes[index], $"GridNode_{x}_{y}");
                }
            }

            //Create GridMap
            var gridMap = new GridMap() {
                GridNodes = gridNodes
            };

            //Set the GridSystemData component
            state.EntityManager.AddComponentData(state.SystemHandle, new GridSystemData() {
                XSize = xSize,
                YSize = ySize,
                CellSize = cellSize,
                GridMap = gridMap
            });
        }

#if !GRID_DEBUG
        [BurstCompile]
#endif
        public void OnUpdate(ref SystemState state) {
            if (Input.GetMouseButtonDown(0)) {
                var position = MouseWorldPosition.Instance.GetPosition();
                var gridSystemData = SystemAPI.GetComponent<GridSystemData>(state.SystemHandle);
                var targetPosition = GridSystemData.GetGridPosition(position, gridSystemData.CellSize);
                if (gridSystemData.IsWithinBounds(targetPosition)) {
                    var gridNodes = InitializeGrid(ref state, gridSystemData, targetPosition);
                    PlaceGridObstacles(ref state, gridSystemData, gridNodes);
                    GenerateFlowField(gridSystemData, targetPosition, gridNodes);
                    gridNodes.Dispose();
                }

                foreach (var (flowFieldFollower, enableFlowFieldFollower) in 
                         SystemAPI.Query<
                             RefRW<FlowFieldFollower>, 
                             EnabledRefRW<FlowFieldFollower>
                         >().WithPresent<FlowFieldFollower>()) {
                    enableFlowFieldFollower.ValueRW = true;
                    flowFieldFollower.ValueRW.TargetPosition = position;
                }

#if GRID_DEBUG
                GridSystemDebug.Instance.UpdateGridDebug(gridSystemData);
#endif
            }
        }

        [BurstCompile]
        private NativeArray<RefRW<GridNode>> InitializeGrid(ref SystemState state, GridSystemData gridSystemData, int2 startingGridPosition) {
            var gridNodes = new NativeArray<RefRW<GridNode>>(gridSystemData.XSize * gridSystemData.YSize, Allocator.Temp);
            for (int y = 0; y < gridSystemData.YSize; y++) {
                for (int x = 0; x < gridSystemData.XSize; x++) {
                    var index = GridSystemData.GetIndex(x, y, gridSystemData.XSize);
                    var gridNode = SystemAPI.GetComponentRW<GridNode>(gridSystemData.GridMap.GridNodes[index]);
                    gridNode.ValueRW.Index = index;
                    if (x == startingGridPosition.x && y == startingGridPosition.y) {
                        gridNode.ValueRW.Cost = 0;
                        gridNode.ValueRW.BestCosts = 0;
                    }
                    else {
                        gridNode.ValueRW.Cost = 1;
                        gridNode.ValueRW.BestCosts = GridSystemData.WALL_COST;
                    }

                    //If stuck, it will eventually move to a valid node
                    gridNode.ValueRW.Vector = new int2(0, 1);
                    gridNodes[index] = gridNode;
                }
            }

            return gridNodes;
        }

        private void PlaceGridObstacles(ref SystemState state, GridSystemData gridSystemData, NativeArray<RefRW<GridNode>> gridNodes) {
            //Take the entities colliders and mark the grid nodes the span as obstacles
            var pws = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
            var collisionWorld = pws.CollisionWorld;

            var collisionFilter = new CollisionFilter() {
                BelongsTo = ~0u,
                CollidesWith = GameConstants.StaticObstacles,
                // CollidesWith = 1u << 10 | 1u << 9,
                GroupIndex = 0
            };

            //TODO Confirm which method is faster, a full AABB collider overlap for the size of the grid or going through the grid nodes and using a sphere/box collider for each one
            //Method 1. Iterating through the grid nodes using sphere collider
            NativeList<DistanceHit> distanceHits = new NativeList<DistanceHit>(Allocator.Temp);
            for (var y = 0; y < gridSystemData.YSize; y++) {
                for (var x = 0; x < gridSystemData.XSize; x++) {
                    var worldPosition = GridSystemData.GetWorldCenterPosition(x, y, gridSystemData.CellSize);
                    if (collisionWorld.OverlapSphere(worldPosition, gridSystemData.CellSize * .5f, ref distanceHits, collisionFilter)) {
                        var index = GridSystemData.GetIndex(x, y, gridSystemData.XSize);
                        gridNodes[index].ValueRW.Cost = GridSystemData.WALL_COST;
                    }
                }
            }

            distanceHits.Clear();
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

                    var newCost = (byte)(currentGridNode.ValueRO.BestCosts + neighbourGridNode.ValueRO.Cost);
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
            //Destroy GridMap
            var gridSystemData = SystemAPI.GetComponent<GridSystemData>(state.SystemHandle);
            state.EntityManager.DestroyEntity(gridSystemData.GridMap.GridNodes);
            gridSystemData.GridMap.GridNodes.Dispose();
        }
    }


    /*
     *                     var aabb = new Aabb() {
           Min = worldPosition - new float3(gridSystemData.CellSize * .5f),
           Max = worldPosition + new float3(gridSystemData.CellSize * .5f)
       };
       var filter = new CollisionFilter() {
           BelongsTo = 1,
           CollidesWith = (uint) 1 << 10,
           GroupIndex = 0
       };
       var input = new OverlapAabbInput() {
           Aabb = aabb,
           Filter = filter
       };
       collisionWorld.OverlapAabb(input, ref distanceHits);
       if (distanceHits.Length > 0) {
           gridNode.ValueRW.Cost = WALL_COST;
           gridNodes[index] = gridNode;
       }
       distanceHits.Clear();
     */
}