using rts.components;
using Unity.Entities;
using UnityEngine;

namespace rts.authoring {
    public class SelectableAuthoring : MonoBehaviour {

        [SerializeField] private bool isSelected;
        [SerializeField] GameObject visualGameObject;
        private class SelectableAuthoringBaker : Baker<SelectableAuthoring> {
            public override void Bake(SelectableAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new Selected {
                    VisualEntity = authoring.visualGameObject != null ? GetEntity(authoring.visualGameObject, TransformUsageFlags.Dynamic) : Entity.Null
                });
                SetComponentEnabled<Selected>(entity, authoring.isSelected);
            }
        }
    }
}