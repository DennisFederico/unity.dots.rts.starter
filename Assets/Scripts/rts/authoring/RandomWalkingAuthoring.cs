using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace rts.authoring {
    public class RandomWalkingAuthoring : MonoBehaviour {

        [SerializeField] private float maxDistance;
        [SerializeField] private float minDistance;
        
        private class RandomWalkingAuthoringBaker : Baker<RandomWalkingAuthoring> {
            public override void Bake(RandomWalkingAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new RandomWalking() {
                    OriginPosition = authoring.transform.position,
                    TargetPosition = authoring.transform.position,
                    DistanceMax = authoring.maxDistance,
                    DistanceMin = authoring.minDistance,
                    RandomSeed = Random.CreateFromIndex((uint)UnityEngine.Random.Range(0, int.MaxValue))
                });
            }
        }
    }

    public struct RandomWalking : IComponentData {
        public float3 OriginPosition;
        public float3 TargetPosition;
        public float DistanceMax;
        public float DistanceMin;
        public Random RandomSeed;
    }
}