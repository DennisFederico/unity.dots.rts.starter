using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

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
    
    public struct AttackTarget : IComponentData {
        public Entity Value;
    }
    
    public struct ShootAttack : IComponentData {
        public float Cooldown;
        public float CooldownTimer;
        public int Damage;
    }
    
    public struct Health : IComponentData {
        public int Value;
    }
}