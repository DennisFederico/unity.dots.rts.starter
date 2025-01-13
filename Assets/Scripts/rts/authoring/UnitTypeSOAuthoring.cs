using rts.scriptable;
using Unity.Entities;
using UnityEngine;

namespace rts.authoring {
    public class UnitTypeSOAuthoring : MonoBehaviour {
        [SerializeField] private UnitTypeSO.UnitType unitType;

        private class UnitTypeSOAuthoringBaker : Baker<UnitTypeSOAuthoring> {
            public override void Bake(UnitTypeSOAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new UnitType {
                    Value = authoring.unitType
                });
            }
        }

        public struct UnitType : IComponentData {
            public UnitTypeSO.UnitType Value;
        }
    }
}