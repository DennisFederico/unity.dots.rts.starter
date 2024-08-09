using rts.components;
using Unity.Entities;
using UnityEngine;

namespace rts.authoring {
    public class AttackTargetOffsetAuthoring : MonoBehaviour {

        [SerializeField] private Transform attackTargetPosition;
        private class AttackTargetOffsetAuthoringBaker : Baker<AttackTargetOffsetAuthoring> {
            public override void Bake(AttackTargetOffsetAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new AttackTargetOffset {
                    Value = authoring.attackTargetPosition.localPosition
                });
            }
        }
    }
}