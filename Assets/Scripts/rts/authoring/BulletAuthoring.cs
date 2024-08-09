using rts.components;
using Unity.Entities;
using UnityEngine;

namespace rts.authoring {
    public class BulletAuthoring : MonoBehaviour {

        [SerializeField] private float bulletSpeed;

        private class BulletAuthoringBaker : Baker<BulletAuthoring> {
            public override void Bake(BulletAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new BulletData() {
                    Speed = authoring.bulletSpeed
                });
                AddComponent<Target>(entity);
            }
        }
    }
}