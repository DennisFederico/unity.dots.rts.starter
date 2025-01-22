using UnityEngine;

namespace rts.mono {
    public class GridSystemDebugCell : MonoBehaviour {
        
        public void SetColor(Color color) {
            
            GetComponentInChildren<SpriteRenderer>().color = color;
        }
    }
}