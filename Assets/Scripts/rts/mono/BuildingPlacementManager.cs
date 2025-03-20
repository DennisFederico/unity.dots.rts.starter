using System;
using rts.authoring;
using rts.scriptable;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.EventSystems;
using BoxCollider = UnityEngine.BoxCollider;
using Material = UnityEngine.Material;

namespace rts.mono {
    public class BuildingPlacementManager : MonoBehaviour {

        public static BuildingPlacementManager Instance { get; private set; }
        
        public event EventHandler OnActiveBuildingTypeSOChanged;
        
        [SerializeField] private BuildingTypeSO activeBuildingSO;
        [SerializeField] private Material ghostMaterial;
        
        private Transform activeBuildingGhost;
        
        private EntityManager entityManager;
        private EntityReferences entityReferences;

        public BuildingTypeSO ActiveBuildingSO {
            get => activeBuildingSO;
            set {
                activeBuildingSO = value;
                //TODO Move the spawn ghost to a method and split the property into methods??
                if (activeBuildingGhost != null) {
                    Destroy(activeBuildingGhost.gameObject);
                }
                if (!value.IsNone) {
                    activeBuildingGhost = Instantiate(value.buildingGhost);
                    foreach (var meshRenderer in activeBuildingGhost.GetComponentsInChildren<MeshRenderer>()) {
                        meshRenderer.material = ghostMaterial;
                    }
                }
                OnActiveBuildingTypeSOChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void Awake() {
            Instance = this;
        }

        private void Start() {
            entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            var entityQuery = entityManager.CreateEntityQuery(typeof(EntityReferences));
            entityReferences = entityQuery.GetSingleton<EntityReferences>();
        }

        private void Update() {
            
            //Update Building ghost position
            if (activeBuildingGhost != null) {
                var targetPosition = MouseWorldPosition.Instance.GetPosition();
                activeBuildingGhost.position = targetPosition;
                //TODO Here we can switch the ghost color to red if the building can't be placed
                //activeBuildingGhost.gameObject.SetActive(CanPlaceBuilding(targetPosition, activeBuildingGhost.GetComponent<BoxCollider>()));
            }

            if (EventSystem.current.IsPointerOverGameObject()) return;
            if (ActiveBuildingSO.IsNone) return;
            
            if (Input.GetMouseButtonUp(1)) {
                ActiveBuildingSO = GameConstants.Instance.BuildingTypeListSO.none;
            }

            if (Input.GetMouseButtonUp(0)) {
                var targetPosition = MouseWorldPosition.Instance.GetPosition();
                var boxCollider = ActiveBuildingSO.prefab.GetComponent<BoxCollider>();

                if (CanPlaceBuilding(targetPosition, boxCollider)) {
                    var entity = entityManager.Instantiate(entityReferences.GetPrefabBuildingForType(activeBuildingSO.buildingType));
                    entityManager.SetComponentData(entity, LocalTransform.FromPosition(targetPosition));    
                }
            }
        }

        private bool CanPlaceBuilding(Vector3 targetPosition, BoxCollider targetCollider) {
            var collisionFilter = new CollisionFilter {
                BelongsTo = ~0u,
                // CollidesWith = GameConstants.Selectable,
                CollidesWith = (uint)(1 << GameConstants.SOLDIERS_LAYER | 1 << GameConstants.ZOMBIES_LAYER | 1 << GameConstants.UNITS_LAYER),
                GroupIndex = 0
            };
            
            float bonusExtents = 1.05f; // 5% bigger than the actual building
            var pws = entityManager.CreateEntityQuery(typeof(PhysicsWorldSingleton)).GetSingleton<PhysicsWorldSingleton>();
            var hitsList = new NativeList<DistanceHit>(Allocator.Temp);
            if (pws.OverlapBox(
                    targetPosition, 
                    Quaternion.identity, 
                    targetCollider.size * .5f * bonusExtents, 
                    ref hitsList, collisionFilter)
                ) return false;
            return true;
        }
    }
}