using UnityEngine;

namespace rts.mono {
    public class GridSystemDebugCell : MonoBehaviour {
        public void SetColor(Color color) {
            GetComponentInChildren<SpriteRenderer>().color = color;
        }

        public void SetSprite(Sprite sprite) {
            GetComponentInChildren<SpriteRenderer>().sprite = sprite;
        }

        public void SetSpriteRotation(Quaternion rotation) {
            GetComponentInChildren<SpriteRenderer>().transform.rotation = rotation * Quaternion.Euler(90, 0, 90);
        }
    }
}