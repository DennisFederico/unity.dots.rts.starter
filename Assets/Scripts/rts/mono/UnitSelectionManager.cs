using System;
using rts.components;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

namespace rts.mono {
    public class UnitSelectionManager : MonoBehaviour {

        public static UnitSelectionManager Instance { get; private set; }

        public event EventHandler OnSelectionStart;
        public event EventHandler OnSelectionEnd;

        private EntityManager _entityManager;
        private Vector2 _selectionStartPosition;
        private Camera _currentCamera;

        private void Awake() {
            Instance = this;
        }

        private void Start() {
            _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            _currentCamera = Camera.main;
        }

        private void Update() {
            if (Input.GetMouseButtonDown(0)) {
                _selectionStartPosition = Input.mousePosition;
                OnSelectionStart?.Invoke(this, EventArgs.Empty);
            }

            if (Input.GetMouseButtonUp(0)) {
                // Vector2 selectionEnd = Input.mousePosition;

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

                if (selectionArea > minSelectionArea) {
                    var entityQuery = new EntityQueryBuilder(Allocator.Temp)
                        .WithAll<UnitTag, LocalTransform>()
                        .WithPresent<Selected>()
                        .Build(_entityManager);


                    var entityArray = entityQuery.ToEntityArray(Allocator.Temp);
                    var positions = entityQuery.ToComponentDataArray<LocalTransform>(Allocator.Temp);
                    for (var i = 0; i < positions.Length; i++) {
                        Vector2 unitScreenPoint = _currentCamera.WorldToScreenPoint(positions[i].Position);
                        if (selectionRect.Contains(unitScreenPoint)) {
                            _entityManager.SetComponentEnabled<Selected>(entityArray[i], true);
                        }
                    }
                } else {
                    var pws = _entityManager.CreateEntityQuery(typeof(PhysicsWorldSingleton)).GetSingleton<PhysicsWorldSingleton>();
                    var screenPointRay = _currentCamera.ScreenPointToRay(Input.mousePosition);
                    var rayCastInput = new RaycastInput {
                        Start = screenPointRay.origin,
                        End = screenPointRay.GetPoint(1000f),
                        Filter = new CollisionFilter {
                            BelongsTo = ~0u,
                            CollidesWith = GameConstants.Selectable,
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
                //RIGHT CLICK A ZOMBIE?
                var pws = _entityManager.CreateEntityQuery(typeof(PhysicsWorldSingleton)).GetSingleton<PhysicsWorldSingleton>();
                var screenPointRay = _currentCamera.ScreenPointToRay(Input.mousePosition);
                var rayCastInput = new RaycastInput {
                    Start = screenPointRay.origin,
                    End = screenPointRay.GetPoint(1000f),
                    Filter = new CollisionFilter {
                        BelongsTo = ~0u,
                        CollidesWith = GameConstants.Zombie,
                        GroupIndex = 0
                    }
                };
                if (pws.CastRay(rayCastInput, out var hit)) {
                    var offset = _entityManager.HasComponent<AttackTargetOffset>(hit.Entity) ?
                        _entityManager.GetComponentData<AttackTargetOffset>(hit.Entity).Value :
                        new float3(0, 0, 1.5f);

                    //Check Faction or any other attribute on the selected target?
                    var selectedWithTargetOverride = new EntityQueryBuilder(Allocator.Temp)
                        .WithAll<Selected>().WithPresent<TargetOverride, ShouldMove>()
                        .Build(_entityManager);
                    var targetOverrides = selectedWithTargetOverride.ToComponentDataArray<TargetOverride>(Allocator.Temp);
                    for (var i = 0; i < targetOverrides.Length; i++) {
                        targetOverrides[i] = new TargetOverride() {
                            Value = hit.Entity,
                            AttackOffset = offset
                        };
                    }
                    selectedWithTargetOverride.CopyFromComponentDataArray(targetOverrides);
                    _entityManager.SetComponentEnabled<ShouldMove>(selectedWithTargetOverride, false);
                } else {
                    //Try to move...
                    var targetPosition = MouseWorldPosition.Instance.GetPosition();
                    //TODO LOOK FOR WAYS TO IMPROVE MEM ALLOCATION IF WE KEEP THIS APPROACH
                    var entityQuery = new EntityQueryBuilder(Allocator.Temp)
                        .WithAll<MoveData, MoveDestination, Selected>().WithPresent<ShouldMove, TargetOverride>()
                        .Build(_entityManager);

                    var destinationArray = entityQuery.ToComponentDataArray<MoveDestination>(Allocator.Temp);
                    var targetPositions = GenerateRandomRingsTargetPositions(destinationArray.Length, targetPosition);
                    var targetOverrides = entityQuery.ToComponentDataArray<TargetOverride>(Allocator.Temp);

                    for (var i = 0; i < destinationArray.Length; i++) {
                        destinationArray[i] = new MoveDestination { Value = targetPositions[i] };
                        targetOverrides[i] = new TargetOverride() { Value = Entity.Null };
                    }
                    entityQuery.CopyFromComponentDataArray(destinationArray);
                    _entityManager.SetComponentEnabled<ShouldMove>(entityQuery, true);
                    entityQuery.CopyFromComponentDataArray(targetOverrides);
                }
            }
        }

        public Rect GetSelectionRect() {
            Vector2 selectionEndPosition = Input.mousePosition;

            var lowerLeftCorner = new Vector2(
                math.min(_selectionStartPosition.x, selectionEndPosition.x),
                math.min(_selectionStartPosition.y, selectionEndPosition.y)
            );
            var upperRightCorner = new Vector2(
                math.max(_selectionStartPosition.x, selectionEndPosition.x),
                math.max(_selectionStartPosition.y, selectionEndPosition.y)
            );

            return new Rect(
                lowerLeftCorner.x,
                lowerLeftCorner.y,
                upperRightCorner.x - lowerLeftCorner.x,
                upperRightCorner.y - lowerLeftCorner.y
            );
        }

        // private NativeArray<float3> GenerateCircleFormationMoveTargetPosition(int count, float3 center, float radius) {
        //     var positions = new NativeArray<float3>(count, Allocator.Temp);
        //     
        //     var angleStep = (math.PI2 / count);
        //     for (var i = 0; i < count; i++) {
        //         var angle = i * angleStep;
        //         var x = center.x + radius * math.cos(angle);
        //         var z = center.z + radius * math.sin(angle);
        //         positions[i] = new float3(x, center.y, z);
        //     }
        //
        //     return positions;
        // }

        // private NativeArray<float3> GenerateRandomMoveTargetPositions(int count, float3 target) {
        //     var positions = new NativeArray<float3>(count, Allocator.Temp);
        //     if (count == 0) return positions;
        //     
        //     //First position is the actual target
        //     positions[0] = target;
        //     int currentIndex = 1;
        //     while (currentIndex < count) {
        //         var randomPosition = new float3(
        //             target.x + UnityEngine.Random.Range(-5f, 5f),
        //             target.y,
        //             target.z + UnityEngine.Random.Range(-5f, 5f)
        //         );
        //         positions[currentIndex] = randomPosition;
        //         currentIndex++;
        //     }
        //     
        //     return positions;
        // }

        private NativeArray<float3> GenerateRandomRingsTargetPositions(int count, float3 target) {
            var positions = new NativeArray<float3>(count, Allocator.Temp);
            if (count == 0) return positions;

            //First position is the actual target
            positions[0] = target;
            int currentIndex = 1;

            //First ring will have 4 positions, and each additional ring will have 2 more positions than the previous ring
            float ringRadius = 2.8f;
            int currentRing = 0;
            int allocatablePositions = 0;
            float angleStep = 0f;
            float3 initialVector = 0f;

            while (currentIndex < count) {
                if (allocatablePositions == 0) {
                    allocatablePositions = 4 + 2 * currentRing;
                    currentRing++;
                    var initialAngle = UnityEngine.Random.Range(0f, math.PIHALF);
                    initialVector = new float3 {
                        x = ringRadius * currentRing * math.cos(initialAngle),
                        y = 0,
                        z = ringRadius * currentRing * math.sin(initialAngle)
                    };
                    angleStep = math.PI2 / allocatablePositions;
                }

                var ringVector = math.rotate(quaternion.RotateY(allocatablePositions * angleStep), initialVector);
                positions[currentIndex] = target + ringVector;
                currentIndex++;
                allocatablePositions--;
            }

            return positions;
        }
    }
}