using UnityEngine;

namespace rts.scriptable {
    
    [CreateAssetMenu()]
    public class AnimationDataSO : ScriptableObject {
        
        public enum AnimationType {
            None,
            SoldierIdle,
            SoldierWalk,
            ZombieIdle,
            ZombieWalk
        }
        
        public AnimationType animationType;
        public Mesh[] meshes;
        public float timerMax;
    }
}