using rts.systems;
using Unity.Entities;
using UnityEngine;
using static rts.systems.GridSystem;

namespace rts.mono {
    public class GridSystemDebug : MonoBehaviour {

        public static GridSystemDebug Instance;
        
        [SerializeField] private Transform gridNodePrefab;
        [SerializeField] Sprite circleSprite;
        [SerializeField] Sprite arrowSprite;
        
        private EntityManager entityManager;
        private GridSystemDebugCell[,] gridSystemDebugCells;
        
        private void Awake() {
            if (Instance == null) {
                Instance = this;
            } else {
                Destroy(gameObject);
            }
            entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        }

        private void Start() {
            var existingSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystem<GridSystem>();
            var gridSystemData = entityManager.GetComponentData<GridSystemData>(existingSystem);
            Initialize(gridSystemData);
        }

        public void Initialize(GridSystemData gridSystemData) {
            gridSystemDebugCells = new GridSystemDebugCell[gridSystemData.XSize, gridSystemData.YSize];
            for (int y = 0; y < gridSystemData.YSize; y++) {
                for (int x = 0; x < gridSystemData.XSize; x++) {
                    var index = GridSystemData.GetIndex(x, y, gridSystemData.XSize);
                    var gridPosition = new Vector3(x * gridSystemData.CellSize, 0.01f, y * gridSystemData.CellSize);
                    var gridNode = Instantiate(gridNodePrefab, gridPosition, Quaternion.identity);
                    gridNode.localScale = new Vector3(gridSystemData.CellSize, gridSystemData.CellSize, gridSystemData.CellSize);
                    gridNode.name = $"GridNode_{index}";
                    var gridSystemDebugCell = gridNode.gameObject.AddComponent<GridSystemDebugCell>();
                    gridSystemDebugCells[x,y] = gridSystemDebugCell;
                }
            }
        }
        
        public void UpdateGridDebug(GridSystemData gridSystemData) {
            for (int y = 0; y < gridSystemData.YSize; y++) {
                for (int x = 0; x < gridSystemData.XSize; x++) {
                    var gridMapGridNode = gridSystemData.GridMap.GridNodes[GridSystemData.GetIndex(x, y, gridSystemData.XSize)];
                    var gridNode = entityManager.GetComponentData<GridNode>(gridMapGridNode);
                    var gridSystemDebugCell = gridSystemDebugCells[x, y];
                    if (gridNode.Cost == 0) {
                        //TargetNode
                        gridSystemDebugCell.SetSprite(circleSprite);
                        gridSystemDebugCell.SetColor(Color.green);
                    } else if (gridNode.Cost == byte.MaxValue) {
                        //ObstacleNode
                        gridSystemDebugCell.SetSprite(circleSprite);
                        gridSystemDebugCell.SetColor(Color.red);
                    } else {
                        //NormalNode
                        gridSystemDebugCell.SetSprite(arrowSprite);
                        gridSystemDebugCell.SetSpriteRotation(Quaternion.LookRotation(new Vector3(gridNode.Vector.x, 0, gridNode.Vector.y), Vector3.up));
                        gridSystemDebugCell.SetColor(Color.white);
                    }            
                }
            }
        }
    }
}