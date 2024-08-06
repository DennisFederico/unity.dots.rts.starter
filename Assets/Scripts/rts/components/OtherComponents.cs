using Unity.Entities;

namespace rts.components {
    public struct BulletData : IComponentData {
        public float Speed;
        public int Damage;
    }
}