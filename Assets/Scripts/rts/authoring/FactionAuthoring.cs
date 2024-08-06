using System;
using rts.components;
using Unity.Entities;
using UnityEngine;

namespace rts.authoring {
    public class FactionAuthoring : MonoBehaviour {

        [SerializeField] private Faction faction;
        private class FactionAuthoringBaker : Baker<FactionAuthoring> {
            public override void Bake(FactionAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new FactionComponent {
                    Value = authoring.faction
                });
                switch (authoring.faction) {
                    case Faction.Friendly:
                        AddComponent(entity, new FriendlyTag());
                        break;
                    case Faction.Enemy:
                        AddComponent(entity, new EnemyTag());
                        break;
                    default:
                        throw new ArgumentOutOfRangeException($"invalid faction {authoring.faction}");
                }
            }
        }
    }
}