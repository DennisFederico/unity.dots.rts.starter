using UnityEditor;
using UnityEngine;

namespace rts.mono {
    
    // It uses Animator.Update(); to manually update the animator, then skinnedMeshRenderer.BakeMesh(mesh); to bake the current state onto a mesh
    // and finally AssetDatabase.CreateAsset(mesh, "Assets/MeshBakeOutput/" + animationName + "_" + frame + ".asset"); to create the mesh asset
    public class BakeAnimationMesh : MonoBehaviour {
        [SerializeField] private Animator animator;
        [SerializeField] private SkinnedMeshRenderer skinnedMeshRenderer;
        [SerializeField] private int frameCount;
        [SerializeField] private float timePerFrame;
        [SerializeField] private string animationName = "Soldier_Idle";

        private void Start() {
            animator.Update(0f);

            for (int frame = 0; frame < frameCount; frame++) {
                Mesh mesh = new Mesh();
                skinnedMeshRenderer.BakeMesh(mesh);

                AssetDatabase.CreateAsset(mesh, "Assets/MeshBakeOutput/" + animationName + "_" + frame + ".asset");

                animator.Update(timePerFrame);
            }
        }
    }
}