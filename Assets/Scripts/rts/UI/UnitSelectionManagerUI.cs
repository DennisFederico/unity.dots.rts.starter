using System;
using rts.mono;
using UnityEngine;

namespace rts.UI {
    public class UnitSelectionManagerUI : MonoBehaviour {
        [SerializeField] private RectTransform unitSelectionArea;
        [SerializeField] private Canvas selectionCanvas;

        private void Start() {
            UnitSelectionManager.Instance.OnSelectionStart += UnitSelectionManagerOnSelectionStart;
            UnitSelectionManager.Instance.OnSelectionEnd += UnitSelectionManagerOnSelectionEnd;
            unitSelectionArea.gameObject.SetActive(false);
        }

        private void Update() {
            if (unitSelectionArea.gameObject.activeSelf) {
                UpdateVisual();
            }
        }

        private void UnitSelectionManagerOnSelectionStart(object sender, EventArgs e) {
            unitSelectionArea.gameObject.SetActive(true);
        }
        
        private void UnitSelectionManagerOnSelectionEnd(object sender, EventArgs e) {
            unitSelectionArea.gameObject.SetActive(false);
            unitSelectionArea.sizeDelta = Vector2.zero;
        }

        private void UpdateVisual() {
            var canvasScale = selectionCanvas.transform.localScale.x;
            var selectionRect = UnitSelectionManager.Instance.GetSelectionRect();
            unitSelectionArea.anchoredPosition = selectionRect.min / canvasScale;
            unitSelectionArea.sizeDelta = selectionRect.size / canvasScale; 
        }
    }
}