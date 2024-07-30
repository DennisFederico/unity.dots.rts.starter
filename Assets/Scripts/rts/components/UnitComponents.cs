using Unity.Entities;
using Unity.Mathematics;

namespace rts.components {
    public struct MoveData : IComponentData {
        public float MoveSpeed;
        public float RotationSpeed;
    }

    public struct ShouldMove : IComponentData, IEnableableComponent {
    }
    
    public struct MoveDestination : IComponentData {
        public float3 Value;
    }
    
    public struct UnitTag : IComponentData {
    }
    
    public struct Selected : IComponentData, IEnableableComponent {
        public Entity VisualEntity;
    }

}