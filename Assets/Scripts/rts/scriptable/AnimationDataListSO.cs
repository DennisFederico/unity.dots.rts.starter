using System.Collections.Generic;
using UnityEngine;

namespace rts.scriptable {
    [CreateAssetMenu()]
    public class AnimationDataListSO : ScriptableObject {
        public List<AnimationDataSO> animationDataSOList;
        
        
        public AnimationDataSO GetAnimationDataSO(AnimationDataSO.AnimationType animationType) {
            foreach (var animationDataSO in animationDataSOList) {
                if (animationDataSO.animationType == animationType) {
                    return animationDataSO;
                }
            }
            Debug.Log($"Could not find a AnimationDataSO with the AnimationType {animationType}");
            return null;
        }
    }
}