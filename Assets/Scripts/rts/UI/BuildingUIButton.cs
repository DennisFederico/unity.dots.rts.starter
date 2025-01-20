using rts.mono;
using rts.scriptable;
using UnityEngine;
using UnityEngine.UI;

namespace rts.UI {
    public class BuildingUIButton : MonoBehaviour {
        
        [SerializeField] private Image iconImage;
        [SerializeField] private Image selectedOutline;
        private BuildingTypeSO buildingTypeSO;
        
        public void Setup(BuildingTypeSO buildingTypeSo) {
            this.buildingTypeSO = buildingTypeSo;
            
            GetComponent<Button>().onClick.AddListener(() => {
                BuildingPlacementManager.Instance.ActiveBuildingSO = buildingTypeSO;
            });
            
            iconImage.sprite = buildingTypeSO.sprite;
            Deselect();
        }
        
        
        public void Select() {
            selectedOutline.enabled = true;
        }
        
        public void Deselect() {
            selectedOutline.enabled = false;
        }
    }
}