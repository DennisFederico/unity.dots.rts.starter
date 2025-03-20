using rts.authoring;
using rts.components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace rts.systems {
    public partial struct RandomWalkingSystem : ISystem {
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            foreach (var (
                         randomWalking, 
                         targetPositionQueued,
                         enabledTargetPositionQueued,
                         localTransform
                         ) in SystemAPI.Query<
                         RefRW<RandomWalking>, 
                         RefRW<TargetPositionQueued>,
                         EnabledRefRW<TargetPositionQueued>,
                         RefRO<LocalTransform>
                     >().WithPresent<TargetPositionQueued>()) {
                
                //TODO ... NEW TARGET POSITION CAN LAND ON AN OBSTACLE
                if (math.distancesq(localTransform.ValueRO.Position, randomWalking.ValueRO.TargetPosition) < 1f) {
                    var random = randomWalking.ValueRO.RandomSeed;
                    var randomDirection = math.normalize(new float3(random.NextFloat(-1f, +1f), 0, random.NextFloat(-1f, +1f)));
                    randomWalking.ValueRW.TargetPosition =
                        randomWalking.ValueRO.OriginPosition +
                        randomDirection * random.NextFloat(randomWalking.ValueRO.DistanceMin, randomWalking.ValueRO.DistanceMax);
                    randomWalking.ValueRW.RandomSeed = random;
                    targetPositionQueued.ValueRW.Value = randomWalking.ValueRO.TargetPosition;
                    enabledTargetPositionQueued.ValueRW = true;
                }
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
    }
}