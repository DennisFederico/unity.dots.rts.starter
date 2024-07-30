using rts.components;
using Unity.Entities;
using UnityEngine;

namespace rts.authoring {
    public class UnitAuthoring : MonoBehaviour {
        private class UnitBaker : Baker<UnitAuthoring> {
            public override void Bake(UnitAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<UnitTag>(entity);
            }
        }
    }
}