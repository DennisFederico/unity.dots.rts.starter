using rts.components;
using Unity.Entities;
using UnityEngine;

namespace rts.authoring {
    public class SelectableAuthoring : MonoBehaviour {

        [SerializeField] private bool isSelected;
        [SerializeField] private float showScale = 2f;
        [SerializeField] private GameObject visualGameObject;
        private class SelectableAuthoringBaker : Baker<SelectableAuthoring> {
            public override void Bake(SelectableAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new Selected {
                    VisualEntity = authoring.visualGameObject != null ? GetEntity(authoring.visualGameObject, TransformUsageFlags.Dynamic) : Entity.Null,
                    ShowScale = authoring.showScale,
                });
                SetComponentEnabled<Selected>(entity, authoring.isSelected);
            }
        }
    }
}