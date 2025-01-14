using System;
using System.Net;
using rts.authoring;
using rts.scriptable;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.EventSystems;
using BoxCollider = UnityEngine.BoxCollider;

namespace rts.mono {
    public class BuildingPlacementManager : MonoBehaviour {

        [SerializeField] private BuildingTypeSO activeBuildingSO;

        private EntityManager entityManager;
        private EntityReferences entityReferences;

        private void Start() {
            entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            var entityQuery = entityManager.CreateEntityQuery(typeof(EntityReferences));
            entityReferences = entityQuery.GetSingleton<EntityReferences>();
        }

        private void Update() {
            if (Input.GetMouseButtonUp(0)) {
                if (EventSystem.current.IsPointerOverGameObject()) return;
                
                var targetPosition = MouseWorldPosition.Instance.GetPosition();
                var boxCollider = activeBuildingSO.prefab.GetComponent<BoxCollider>();

                if (CanPlaceBuilding(targetPosition, boxCollider)) {
                    var entity = entityManager.Instantiate(entityReferences.BuildingTowerPrefab);
                    entityManager.SetComponentData(entity, LocalTransform.FromPosition(targetPosition));    
                }
            }
        }

        private bool CanPlaceBuilding(Vector3 targetPosition, BoxCollider targetCollider) {
            var collisionFilter = new CollisionFilter {
                BelongsTo = ~0u,
                // CollidesWith = GameConstants.Selectable,
                CollidesWith = (uint)(1 << GameConstants.SoldiersLayer | 1 << GameConstants.ZombiesLayer | 1 << GameConstants.UnitsLayer),
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