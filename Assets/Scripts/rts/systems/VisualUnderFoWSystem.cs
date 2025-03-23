using rts.authoring;
using rts.mono;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

namespace rts.systems {
    public partial struct VisualUnderFoWSystem : ISystem {
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<PhysicsWorldSingleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {

            var collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
            foreach (var (fowVisible, enableMeshInfo) 
                     in SystemAPI.Query<RefRW<VisualUnderFoW>, EnabledRefRW<MaterialMeshInfo>>()
                         .WithPresent<MaterialMeshInfo>()) {
                var origin = SystemAPI.GetComponentRO<LocalTransform>(fowVisible.ValueRO.RootParent).ValueRO.Position;
                if (collisionWorld.SphereCast(
                    origin,
                    fowVisible.ValueRO.SphereCastSize,
                    new float3(0, 1, 0),
                    100,
                    new CollisionFilter() {
                        BelongsTo = ~0u,
                        CollidesWith = 1u << GameConstants.FOW,
                        GroupIndex = 0
                    })) {
                    // VISIBLE, UNDER FOW SIGHT
                    if (fowVisible.ValueRO.IsVisible) continue;
                    fowVisible.ValueRW.IsVisible = true;
                    enableMeshInfo.ValueRW = true;
                } else {
                    // NOT VISIBLE
                    if (!fowVisible.ValueRO.IsVisible) continue;
                    fowVisible.ValueRW.IsVisible = false;
                    enableMeshInfo.ValueRW = false;
                }
            }

            // var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            // foreach (var (fowVisible, parent, entity) 
            //          in SystemAPI.Query<RefRW<VisualUnderFoW>, RefRO<Parent>>()
            //              .WithAbsent<MaterialMeshInfo>()
            //              .WithEntityAccess()) {
            //     Debug.Log($"Check healthbar or icon?? {entity}");
            //     var origin = SystemAPI.GetComponentRO<LocalTransform>(parent.ValueRO.Value).ValueRO.Position;
            //     if (collisionWorld.SphereCast(
            //             origin,
            //             fowVisible.ValueRO.SphereCastSize,
            //             new float3(0, 1, 0),
            //             100,
            //             new CollisionFilter() {
            //                 BelongsTo = ~0u,
            //                 CollidesWith = 1u << GameConstants.FOW,
            //                 GroupIndex = 0
            //             })) {
            //         // VISIBLE, UNDER FOW SIGHT
            //         if (fowVisible.ValueRO.IsVisible) continue;
            //         fowVisible.ValueRW.IsVisible = true;
            //         ecb.RemoveComponent<DisableRendering>(entity);
            //     } else {
            //         // NOT VISIBLE
            //         if (!fowVisible.ValueRO.IsVisible) continue;
            //         fowVisible.ValueRW.IsVisible = false;
            //         ecb.AddComponent<DisableRendering>(entity);
            //     }
            // }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
    }
}