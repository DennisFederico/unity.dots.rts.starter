using Unity.Entities;
using UnityEngine;

namespace rts.authoring
{
    public class AnimatedMeshAuthoring : MonoBehaviour
    {
        private class AnimatedMeshAuthoringBaker : Baker<AnimatedMeshAuthoring>
        {
            public override void Bake(AnimatedMeshAuthoring authoring)
            {
            }
        }
    }
}