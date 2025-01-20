using System;
using System.Collections.Generic;
using rts.mono;
using rts.scriptable;
using UnityEngine;

namespace rts.UI {
    public class BuildingPlacementManagerUI : MonoBehaviour {
        [SerializeField] private RectTransform buttonContainer;
        [SerializeField] private RectTransform buttonTemplate;
        [SerializeField] private BuildingTypeListSO buildingTypeListSO;
        
        private Dictionary<BuildingTypeSO, BuildingUIButton> buildingTypeToButtonMap;
        
        private void Awake() {
            buildingTypeToButtonMap = new Dictionary<BuildingTypeSO, BuildingUIButton>();
            buttonTemplate.gameObject.SetActive(false);
            
            //Loop through all building types and create a button for each
            foreach (var buildingType in buildingTypeListSO.buildingTypeSOList) {
                if (!buildingType.showInBuildUI) continue;
                var button = Instantiate(buttonTemplate, buttonContainer);
                button.GetComponent<BuildingUIButton>().Setup(buildingType);
                button.gameObject.SetActive(true);
                
                buildingTypeToButtonMap[buildingType] = button.GetComponent<BuildingUIButton>();
            }
        }

        private void Start() {
            BuildingPlacementManager.Instance.OnActiveBuildingTypeSOChanged += BuildingPlacementManager_OnActiveBuildingTypeSOChanged;
            UpdateSelectedVisual();
        }

        private void BuildingPlacementManager_OnActiveBuildingTypeSOChanged(object sender, EventArgs e) {
            UpdateSelectedVisual();
        }

        private void UpdateSelectedVisual() {
            foreach (var buildingTypeSO in buildingTypeToButtonMap.Keys) {
                buildingTypeToButtonMap[buildingTypeSO].Deselect();
            }
            buildingTypeToButtonMap[BuildingPlacementManager.Instance.ActiveBuildingSO].Select();
        }
    }
}