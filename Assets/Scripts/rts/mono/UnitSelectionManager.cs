using System;
using rts.components;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using Math = System.Math;

namespace rts.mono {
    public class UnitSelectionManager : MonoBehaviour {

        public static UnitSelectionManager Instance { get; private set; }

        public event EventHandler OnSelectionStart;
        public event EventHandler OnSelectionEnd;

        private EntityManager _entityManager;
        private Vector2 _selectionStartPosition;

        private void Awake() {
            Instance = this;
        }

        private void Start() {
            _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        }

        private void Update() {
            if (Input.GetMouseButtonDown(0)) {
                _selectionStartPosition = Input.mousePosition;
                OnSelectionStart?.Invoke(this, EventArgs.Empty);
            }

            if (Input.GetMouseButtonUp(0)) {
                Vector2 selectionEnd = Input.mousePosition;

                //DESELECT LOGIC
                var selectedUnits = new EntityQueryBuilder(Allocator.Temp)
                    .WithAll<UnitTag, Selected>()
                    .Build(_entityManager);
                foreach (var selectedUnit in selectedUnits.ToEntityArray(Allocator.Temp)) {
                    _entityManager.SetComponentEnabled<Selected>(selectedUnit, false);
                }

                //HANDLE SELECTION TYPE (SINGLE OR MULTIPLE)
                var selectionRect = GetSelectionRect();
                float minSelectionArea = 300f;
                var selectionArea = selectionRect.width * selectionRect.height;

                //Debug.Log($"Selection Area {selectionArea} - Single: {selectionArea < minSelectionArea}");
                if (selectionArea > minSelectionArea) {
                    var entityQuery = new EntityQueryBuilder(Allocator.Temp)
                        .WithAll<UnitTag, LocalTransform>()
                        .WithPresent<Selected, ShouldMove>()
                        .Build(_entityManager);


                    var entityArray = entityQuery.ToEntityArray(Allocator.Temp);
                    var positions = entityQuery.ToComponentDataArray<LocalTransform>(Allocator.Temp);
                    for (var i = 0; i < positions.Length; i++) {
                        Vector2 unitScreenPoint = Camera.main.WorldToScreenPoint(positions[i].Position);
                        if (selectionRect.Contains(unitScreenPoint)) {
                            _entityManager.SetComponentEnabled<Selected>(entityArray[i], true);
                        }
                    }

                    // entityQuery.CopyFromComponentDataArray(positions);
                    // _entityManager.SetComponentEnabled<ShouldMove>(entityQuery, true);
                } else {
                    var pws = _entityManager.CreateEntityQuery(typeof(PhysicsWorldSingleton))
                        .GetSingleton<PhysicsWorldSingleton>();
                    //pws.CollisionWorld.CastRay();
                    var screenPointRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                    var rayCastInput = new RaycastInput {
                        Start = screenPointRay.origin,
                        End = screenPointRay.GetPoint(1000f),
                        Filter = new CollisionFilter {
                            BelongsTo = ~0u,
                            CollidesWith = 1u << 6,
                            GroupIndex = 0
                        }
                    };
                    if (pws.CastRay(rayCastInput, out var closestHit) && _entityManager.HasComponent<UnitTag>(closestHit.Entity)) {
                        _entityManager.SetComponentEnabled<Selected>(closestHit.Entity, true);
                    }
                    
                }

                OnSelectionEnd?.Invoke(this, EventArgs.Empty);
            }

            if (Input.GetMouseButtonDown(1)) {
                var targetPosition = MouseWorldPosition.Instance.GetPosition();
                //TODO LOOK FOR WAYS TO IMPROVE MEM ALLOCATION IF WE KEEP THIS APPROACH
                var entityQuery = new EntityQueryBuilder(Allocator.Temp)
                    .WithAll<MoveData, MoveDestination, Selected>()
                    .WithPresent<ShouldMove>()
                    .Build(_entityManager);

                var destinationArray = entityQuery.ToComponentDataArray<MoveDestination>(Allocator.Temp);
                for (var i = 0; i < destinationArray.Length; i++) {
                    destinationArray[i] = new MoveDestination { Value = targetPosition };
                }

                entityQuery.CopyFromComponentDataArray(destinationArray);
                _entityManager.SetComponentEnabled<ShouldMove>(entityQuery, true);

                // var entityArray = entityQuery.ToEntityArray(Allocator.Temp);
                // foreach (var entity in entityArray) {
                //     _entityManager.SetComponentData(entity, new MoveDestination { Value = targetPosition });
                //     _entityManager.SetComponentEnabled<ShouldMove>(entity, true);
                // }
            }
        }

        public Rect GetSelectionRect() {
            Vector2 selectionEndPosition = Input.mousePosition;

            var lowerLeftCorner = new Vector2(
                Math.Min(_selectionStartPosition.x, selectionEndPosition.x),
                Math.Min(_selectionStartPosition.y, selectionEndPosition.y)
            );
            var upperRightCorner = new Vector2(
                Math.Max(_selectionStartPosition.x, selectionEndPosition.x),
                Math.Max(_selectionStartPosition.y, selectionEndPosition.y)
            );

            return new Rect(
                lowerLeftCorner.x,
                lowerLeftCorner.y,
                upperRightCorner.x - lowerLeftCorner.x,
                upperRightCorner.y - lowerLeftCorner.y
            );
        }
    }
}