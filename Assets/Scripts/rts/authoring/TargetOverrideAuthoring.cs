using rts.components;
using Unity.Entities;
using UnityEngine;

namespace rts.authoring {
    public class TargetOverrideAuthoring : MonoBehaviour {
        public class TargetOverrideBaker : Baker<TargetOverrideAuthoring> {
            public override void Bake(TargetOverrideAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<TargetOverride>(entity);
            }
        }
    }
}