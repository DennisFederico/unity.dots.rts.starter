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

            public static int GetIndex(int x, int y, int xSize) {
                return x + y * xSize;
            }
            
            public static int GetGridIndex(int2 gridPosition, int xSize) {
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
                    (int) math.floor(worldPosition.x / cellSize), 
                    (int) math.floor(worldPosition.z / cellSize));
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

#if GRID_DEBUG
            //Flip the grid node data on left click
            // if (Input.GetMouseButtonDown(0)) {
            //     var position = MouseWorldPosition.Instance.GetPosition();
            //     var gridSystemData = SystemAPI.GetComponent<GridSystemData>(state.SystemHandle);
            //     var gridPosition = GridSystemData.GetGridPosition(position, gridSystemData.CellSize);
            //     if (gridSystemData.IsWithinBounds(gridPosition)) {
            //         var gridEntity = gridSystemData.GridMap.GridNodes[GridSystemData.GetGridIndex(gridPosition, gridSystemData.XSize)];
            //         var gridNode = SystemAPI.GetComponentRW<GridNode>(gridEntity);
            //         gridNode.ValueRW.Data = (byte) (gridNode.ValueRO.Data == 0 ? 1 : 0);
            //         GridSystemDebug.Instance.UpdateGridDebug(gridSystemData);    
            //     }
            // }
#endif
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