#define GRID_DEBUG
using rts.mono;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace rts.systems {
    public partial struct GridSystem : ISystem {
        public struct GridSystemData : IComponentData {
            public int XSize;
            public int YSize;
            public float CellSize;
            public GridMap GridMap;

            public static int GetIndex(int posX, int posY, int xSize) {
                return posX + posY * xSize;
            }

            public static int GetIndex(int2 gridPosition, int xSize) {
                return gridPosition.x + gridPosition.y * xSize;
            }

            // public static int GetGridIndex(float3 worldPosition, int xSize, float cellSize) {
            //     return GetGridIndex(GetGridPosition(worldPosition, cellSize), xSize);
            // }

            public static int2 GetGridPosition(int index, int xSize) {
                return new int2(index % xSize, index / xSize);
            }

            public static int2 GetGridPosition(float3 worldPosition, float cellSize) {
                return new int2(
                    (int)math.floor(worldPosition.x / cellSize),
                    (int)math.floor(worldPosition.z / cellSize));
            }

            public static float3 GetWorldPosition(int2 gridPosition, float cellSize) {
                return new float3(gridPosition.x * cellSize, 0.01f, gridPosition.y * cellSize);
            }

            public static float3 GetWorldPosition(int index, int xSize, float cellSize) {
                return GetWorldPosition(GetGridPosition(index, xSize), cellSize);
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
        }

        private float elapsedTime;

        [BurstCompile]
        public void OnCreate(ref SystemState state) {
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
                var gridPosition = GridSystemData.GetGridPosition(position, gridSystemData.CellSize);
                if (gridSystemData.IsWithinBounds(gridPosition)) {
                    var gridNodes = InitializeGrid(ref state, gridSystemData, gridPosition);
                    //TODO MAKE FLOWFIELD2 USE ARRAY OF INT2 LIKE FLOWFIELD, DONT STORE THAT MANY COPIES OF COMPONENTS
                    // GenerateFlowField(gridSystemData, gridPosition, gridNodes);
                    GenerateFlowField2(gridSystemData, gridPosition, gridNodes);
                    gridNodes.Dispose();
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
                        gridNode.ValueRW.BestCosts = byte.MaxValue;
                    }

                    //If stuck, it will eventually move to a valid node
                    gridNode.ValueRW.Vector = new int2(0, 1);
                    gridNodes[index] = gridNode;
                }
            }

            return gridNodes;
        }

        private void GenerateFlowField2(GridSystemData gridSystemData, int2 startingGridPosition, NativeArray<RefRW<GridNode>> gridNodes) {
            //Open list
            var openQueue = new NativeQueue<RefRW<GridNode>>(Allocator.Temp);
            //Enqueue the starting node
            openQueue.Enqueue(gridNodes[GridSystemData.GetIndex(startingGridPosition, gridSystemData.XSize)]);

            int safeLimit = (int)(gridSystemData.XSize * gridSystemData.YSize * 1.1f);
            int iterations = 0;

            //While there are nodes in the open list
            while (openQueue.Count > 0) {
                iterations++;
                if (iterations > safeLimit) {
                    Debug.LogError("Safe limit reached");
                    break;
                }

                //Dequeue the current node
                var currentGridNode = openQueue.Dequeue();
                //Visit the neighbours
                for (int y = -1; y <= 1; y++) {
                    for (int x = -1; x <= 1; x++) {
                        if (x == 0 && y == 0) continue; //Skip the current node
                        var neighbourGridPosition = currentGridNode.ValueRO.GridPosition + new int2(x, y);
                        if (!gridSystemData.IsWithinBounds(neighbourGridPosition)) continue; //Skip if out of bounds
                        var neighbourGridIndex = GridSystemData.GetIndex(neighbourGridPosition, gridSystemData.XSize);
                        var neighbourGridNode = gridNodes[neighbourGridIndex];

                        //Calculate the new cost
                        var newCost = (byte)(currentGridNode.ValueRO.BestCosts + neighbourGridNode.ValueRO.Cost);
                        if (newCost < neighbourGridNode.ValueRO.BestCosts) {
                            neighbourGridNode.ValueRW.BestCosts = newCost;
                            neighbourGridNode.ValueRW.Vector = currentGridNode.ValueRO.GridPosition - neighbourGridPosition;
                            openQueue.Enqueue(neighbourGridNode);
                        }
                    }
                }
            }

            openQueue.Dispose();
        }

        [BurstCompile]
        private void GenerateFlowField(GridSystemData gridSystemData, int2 startingGridPosition, NativeArray<RefRW<GridNode>> gridNodes) {
            var openList = new NativeList<int2>(Allocator.Temp);
            openList.Add(startingGridPosition);

            int safeLimit = (int)(gridSystemData.XSize * gridSystemData.YSize * 1.1f);
            int iterations = 0;

            while (openList.Length > 0) {
                iterations++;
                if (iterations > safeLimit) {
                    Debug.LogError("Safe limit reached");
                    break;
                }

                var currentGridPosition = openList[0];
                openList.RemoveAtSwapBack(0);
                // var currentGridIndex = GridSystemData.GetIndex(currentGridPosition, gridSystemData.XSize);
                // var currentGridNode = SystemAPI.GetComponentRW<GridNode>(gridSystemData.GridMap.GridNodes[currentGridIndex]);
                var currentGridNode = gridNodes[GridSystemData.GetIndex(currentGridPosition, gridSystemData.XSize)];

                for (int y = -1; y <= 1; y++) {
                    for (int x = -1; x <= 1; x++) {
                        if (x == 0 && y == 0) continue;
                        var neighbourGridPosition = currentGridPosition + new int2(x, y);
                        if (!gridSystemData.IsWithinBounds(neighbourGridPosition)) continue;

                        var neighbourGridIndex = GridSystemData.GetIndex(neighbourGridPosition, gridSystemData.XSize);
                        // var neighbourGridNode = SystemAPI.GetComponentRW<GridNode>(gridSystemData.GridMap.GridNodes[neighbourGridIndex]);
                        var neighbourGridNode = gridNodes[neighbourGridIndex];

                        var newCost = (byte)(currentGridNode.ValueRO.BestCosts + neighbourGridNode.ValueRO.Cost);
                        if (newCost < neighbourGridNode.ValueRO.BestCosts) {
                            neighbourGridNode.ValueRW.BestCosts = newCost;
                            neighbourGridNode.ValueRW.Vector = currentGridPosition - neighbourGridPosition;
                            openList.Add(neighbourGridPosition);
                        }
                    }
                }
            }

            openList.Dispose();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) {
            //Destroy GridMap
            var gridSystemData = SystemAPI.GetComponent<GridSystemData>(state.SystemHandle);
            state.EntityManager.DestroyEntity(gridSystemData.GridMap.GridNodes);
            gridSystemData.GridMap.GridNodes.Dispose();
        }
    }
}