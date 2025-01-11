using UnityEngine;

namespace rts.scriptable {
    [CreateAssetMenu()]
    public class AnimationDataSO : ScriptableObject {
        public enum AnimationType {
            None,
            SoldierIdle,
            SoldierWalk,
            ZombieIdle,
            ZombieWalk,
            SoldierAim,
            SoldierShoot,
            ZombieMeleeAttack,
            ScoutIdle,
            ScoutWalk,
            ScoutAim,
            ScoutShoot,
        }

        public AnimationType animationType;
        public Mesh[] meshes;
        public float timerMax;

        public static bool IsAnimationUnInterruptible(AnimationType animationType) {
            switch (animationType) {
                default: return false;
                // case AnimationType.SoldierShoot:
                case AnimationType.ZombieMeleeAttack:
                // case AnimationType.ScoutShoot:
                    return true;
            }
        }
    }
}