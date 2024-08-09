using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace rts.components {
    public struct MoveData : IComponentData {
        public float MoveSpeed;
        public float RotationSpeed;
    }

    public struct ShouldMove : IComponentData {
        public bool Value;
    }
    
    public struct MoveDestination : IComponentData {
        public float3 Value;
    }
    
    public struct UnitTag : IComponentData {
        
    }
    
    public struct EnemyTag : IComponentData {
        
    }
    
    public struct FriendlyTag : IComponentData {
        
    }

    public struct FactionComponent : IComponentData {
        public Faction Value;
    }
    
    public struct TargetingData : IComponentData {
        public float Range;
        public LayerMask TargetLayers;
        public Faction TargetFaction;
    }
    
    public struct Selected : IComponentData, IEnableableComponent {
        public Entity VisualEntity;
        public float ShowScale;
    }
    
    public struct Target : IComponentData {
        public Entity Value;
        public float3 AttackOffset;
    }
    
    public struct ShootAttack : IComponentData {
        public float Cooldown;
        public float CooldownTimer;
        public float AttackDistance;
        public float AttackDistanceSquared;
        public int Damage;
        public float3 ProjectileOffset;
    }
    
    public struct Health : IComponentData {
        public int Value;
    }
    
    public struct AttackTargetOffset : IComponentData {
        public float3 Value;
    }
}