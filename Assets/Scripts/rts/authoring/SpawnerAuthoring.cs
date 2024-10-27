using Unity.Entities;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace rts.authoring {
    public class SpawnerAuthoring : MonoBehaviour {
        
        [SerializeField] private GameObject prefab;
        [SerializeField] private float spawnRate;
        [SerializeField] private Vector2 size;

        private void OnDrawGizmos() {
            Gizmos.color = Color.green;

            Vector3 topLeft = new Vector3(-size.x, 0, size.y) + transform.position;
            Vector3 topRight = new Vector3(size.x, 0, size.y)+ transform.position;
            Vector3 bottomLeft = new Vector3(-size.x, 0, -size.y)+ transform.position;
            Vector3 bottomRight = new Vector3(size.x, 0, -size.y)+ transform.position;

            Handles.DrawLine(topLeft, topRight);
            Handles.DrawLine(topRight, bottomRight);
            Handles.DrawLine(bottomRight, bottomLeft);
            Handles.DrawLine(bottomLeft, topLeft);
        }
        
        private class SpawnerAuthoringBaker : Baker<SpawnerAuthoring> {
            public override void Bake(SpawnerAuthoring authoring) {
                DependsOn(authoring.transform);
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new SpawnerData {
                    Position = authoring.transform.position,
                    Area = authoring.size,
                    Prefab = GetEntity(authoring.prefab, TransformUsageFlags.Dynamic),
                    SpawnRate = authoring.spawnRate,
                    SpawnTimer = authoring.spawnRate,
                    Random = new Random((uint) UnityEngine.Random.Range(1, 100000))
                });
            }
        }

        public struct SpawnerData : IComponentData {
            public float3 Position;
            public float2 Area;
            public Entity Prefab;
            public float SpawnRate;
            public float SpawnTimer;
            public Random Random;
        }
    }
}