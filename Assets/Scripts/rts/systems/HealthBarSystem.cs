using rts.authoring;
using rts.components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace rts.systems {
    public partial struct HealthBarSystem : ISystem {
        private ComponentLookup<LocalTransform> localTransformLookup;
        private ComponentLookup<Health> healthLookup;
        private ComponentLookup<PostTransformMatrix> postTransformMatrixLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            localTransformLookup = state.GetComponentLookup<LocalTransform>(false);
            healthLookup = state.GetComponentLookup<Health>(true);
            postTransformMatrixLookup = state.GetComponentLookup<PostTransformMatrix>(false);
        }

        // [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            localTransformLookup.Update(ref state);
            healthLookup.Update(ref state);
            postTransformMatrixLookup.Update(ref state);
            //TODO ... Improve with an event system based in bool flags or enable-able components
            float3 cameraForward = Camera.main != null ? Camera.main.transform.forward : float3.zero;

            new HealthBarJob() {
                HealthLookup = healthLookup,
                LocalTransformLookup = localTransformLookup,
                PostTransformMatrixLookup = postTransformMatrixLookup,
                CameraForward = cameraForward
            }.ScheduleParallel();

            //MOVED TO A JOB
            // foreach (var (localTransform, healthBar) in SystemAPI.Query<RefRW<LocalTransform>, RefRO<HealthBar>>()) {
            //     
            //     //Face camera
            //     var parentTransform = SystemAPI.GetComponent<LocalTransform>(healthBar.ValueRO.BelongsTo);
            //     var localRotation = parentTransform.InverseTransformRotation(quaternion.LookRotationSafe(cameraForward, math.up()));
            //     localTransform.ValueRW.Rotation = localRotation;
            //     
            //     //Need to update value?
            //     var health = SystemAPI.GetComponentRO<Health>(healthBar.ValueRO.BelongsTo);
            //     if (!health.ValueRO.HasChanged) continue;
            //     
            //     
            //     var normalizedHealth = (float) health.ValueRO.Value / health.ValueRO.MaxValue;
            //     localTransform.ValueRW.Scale = normalizedHealth >= 1f ? 0 : 1;
            //     
            //     var postTransformMatrix = SystemAPI.GetComponentRW<PostTransformMatrix>(healthBar.ValueRO.Bar);
            //     postTransformMatrix.ValueRW.Value = float4x4.Scale(normalizedHealth, 1, 1);
            // }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
    }

    public partial struct HealthBarJob : IJobEntity {
        [NativeDisableParallelForRestriction] public ComponentLookup<LocalTransform> LocalTransformLookup;
        [ReadOnly] public ComponentLookup<Health> HealthLookup;
        [NativeDisableParallelForRestriction] public ComponentLookup<PostTransformMatrix> PostTransformMatrixLookup;
        [ReadOnly] public float3 CameraForward;

        public void Execute(in HealthBar healthBar, Entity entity) {
            //Face camera
            var healthBarLocalTransform = LocalTransformLookup.GetRefRW(entity);
            var parentTransform = LocalTransformLookup[healthBar.BelongsTo];
            if (healthBarLocalTransform.ValueRO.Scale >= 1f) {
                healthBarLocalTransform.ValueRW.Rotation = parentTransform.InverseTransformRotation(quaternion.LookRotationSafe(CameraForward, math.up()));    
            }
            
            //Need to update value?
            var health = HealthLookup[healthBar.BelongsTo];
            if (!health.HasChanged) return;
            
            var normalizedHealth = (float)health.Value / health.MaxValue;
            var postTransformMatrix = PostTransformMatrixLookup.GetRefRW(healthBar.Bar);
            postTransformMatrix.ValueRW.Value = float4x4.Scale(normalizedHealth, 1, 1);
            healthBarLocalTransform.ValueRW.Scale = normalizedHealth >= 1f ? 0 : 1;
        }
    }
}