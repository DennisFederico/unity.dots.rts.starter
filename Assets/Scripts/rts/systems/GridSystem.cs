using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace rts.systems {
    public partial struct GridSystem : ISystem {
        
        public struct GridSystemData : IComponentData{
            public int XSize;
            public int YSize;
            public float CellSize;
            public GridMap GridMap;
            
            public static int GetIndex(int x, int y, int xSize) {
                return x + y * xSize;
            }
            
            public int GetIndex(int x, int y) {
                return GetIndex(x, y, XSize);
            }
            
            public int GetIndex(int2 gridPosition) {
                return GetIndex(gridPosition.x, gridPosition.y);
            }
            
            public int2 GetGridPosition(int index) {
                return new int2(index % XSize, index / XSize);
            }
            
            public bool IsWithinBounds(int x, int y) {
                return x >= 0 && x < XSize && y >= 0 && y < YSize;
            }
        }

        public struct GridMap {
            public NativeArray<Entity> GridNodes;
        }
        
        public struct GridNode : IComponentData {
            public int2 GridPosition;
            public byte Data;
        }
        
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

        [BurstCompile]
        public void OnUpdate(ref SystemState state) { }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) {
            //Destroy GridMap
            var gridSystemData = state.EntityManager.GetComponentData<GridSystemData>(state.SystemHandle);
            state.EntityManager.DestroyEntity(gridSystemData.GridMap.GridNodes);
            gridSystemData.GridMap.GridNodes.Dispose();
        }
    }
}