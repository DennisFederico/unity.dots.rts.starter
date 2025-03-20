using rts.components;
using Unity.Entities;
using UnityEngine;

namespace rts.authoring {
    public class MovableAuthoring : MonoBehaviour {
        [SerializeField] private float moveSpeed;
        [SerializeField] private float rotationSpeed;

        public class Baker : Baker<MovableAuthoring> {
            public override void Bake(MovableAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new MoveData {
                    MoveSpeed = authoring.moveSpeed,
                    RotationSpeed = authoring.rotationSpeed
                });
                AddComponent(entity, new MoveDestination() { Value = authoring.transform.position });
                AddComponent<ShouldMove>(entity);
                SetComponentEnabled<ShouldMove>(entity, false);
            }
        }
    }
}