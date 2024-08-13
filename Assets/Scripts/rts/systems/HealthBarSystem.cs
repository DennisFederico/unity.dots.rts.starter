using rts.authoring;
using rts.components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace rts.systems {
    public partial struct HealthBarSystem : ISystem {
        
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {

            //TODO ... Improve with an event system based in bool flags or enable-able components
            
            float3 cameraForward = Camera.main != null ? Camera.main.transform.forward : float3.zero;
            
            foreach (var (localTransform, healthBar) in SystemAPI.Query<RefRW<LocalTransform>, RefRO<HealthBar>>()) {
                
                //Face camera
                var parentTransform = SystemAPI.GetComponent<LocalTransform>(healthBar.ValueRO.BelongsTo);
                var localRotation = parentTransform.InverseTransformRotation(quaternion.LookRotationSafe(cameraForward, math.up()));
                localTransform.ValueRW.Rotation = localRotation;
                
                //Need to update value?
                var health = SystemAPI.GetComponentRO<Health>(healthBar.ValueRO.BelongsTo);
                if (!health.ValueRO.HasChanged) continue;
                
                
                var normalizedHealth = (float) health.ValueRO.Value / health.ValueRO.MaxValue;
                localTransform.ValueRW.Scale = normalizedHealth >= 1f ? 0 : 1;
                
                var postTransformMatrix = SystemAPI.GetComponentRW<PostTransformMatrix>(healthBar.ValueRO.Bar);
                postTransformMatrix.ValueRW.Value = float4x4.Scale(normalizedHealth, 1, 1);
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) {

        }
    }
}