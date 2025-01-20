using rts.components;
using Unity.Entities;
using UnityEngine;

namespace rts.authoring {
    public class TargetSoldiersHQOnSpawnAuthoring : MonoBehaviour {
        private class AttackSoldiersHQOnSpawnAuthoringBaker : Baker<TargetSoldiersHQOnSpawnAuthoring> {
            public override void Bake(TargetSoldiersHQOnSpawnAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<TargetSoldiersHQOnSpawn>(entity);
            }
        }
    }
}