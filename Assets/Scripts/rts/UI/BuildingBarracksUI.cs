using System;
using rts.authoring;
using rts.components;
using rts.mono;
using rts.scriptable;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

namespace rts.UI {
    public class BuildingBarracksUI : MonoBehaviour {
        [SerializeField] private Button soldierButton;
        [SerializeField] private Button scoutButton;
        [SerializeField] private Image progressBar;
        [SerializeField] private RectTransform unitQueueContainer;
        [SerializeField] private RectTransform unitQueueTemplate;

        private EntityManager entityManager;

        //TODO Handle more than one selected barrack
        private Entity selectedBarracks;

        private void Awake() {
            soldierButton.onClick.AddListener(() => {
                entityManager.SetComponentData(selectedBarracks, new BuildingBarracksUnitEnqueue() {
                    UnitType = UnitTypeSO.UnitType.Soldier
                });
                entityManager.SetComponentEnabled<BuildingBarracksUnitEnqueue>(selectedBarracks, true);
            });

            scoutButton.onClick.AddListener(() => {
                entityManager.SetComponentData(selectedBarracks, new BuildingBarracksUnitEnqueue() {
                    UnitType = UnitTypeSO.UnitType.Scout
                });
                entityManager.SetComponentEnabled<BuildingBarracksUnitEnqueue>(selectedBarracks, true);
            });

            unitQueueTemplate.gameObject.SetActive(false);
        }

        private void Start() {
            entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            UnitSelectionManager.Instance.OnSelectedEntitiesChanged += UnitSelectionManager_OnSelectedEntitiesChanged;
            DOTSEventManager.Instance.OnBarracksQueueChanged += HandleBarracksQueueChanged;
            Hide();
        }

        private void HandleBarracksQueueChanged(object sender, EventArgs e) {
            if ((Entity)sender != selectedBarracks) return;
            UpdateUnitQueueVisual();
        }

        private void Update() {
            UpdateProgressBarVisual();
        }

        private void UpdateProgressBarVisual() {
            if (selectedBarracks == Entity.Null) return;
            var buildingState = entityManager.GetComponentData<BuildingBarracksState>(selectedBarracks);
            progressBar.fillAmount = buildingState.Progress / buildingState.ProgressRequired;
        }

        private void UnitSelectionManager_OnSelectedEntitiesChanged(object sender, EventArgs e) {
            var queryForBarracks = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<Selected, BuildingBarracksState>()
                .Build(entityManager);

            var entityArray = queryForBarracks.ToEntityArray(Allocator.Temp);
            if (entityArray.Length > 0) {
                selectedBarracks = entityArray[0];
                Show();
            }
            else {
                selectedBarracks = Entity.Null;
                Hide();
            }
        }

        private void UpdateUnitQueueVisual() {
            foreach (Transform child in unitQueueContainer) {
                if (child == unitQueueTemplate) continue;
                Destroy(child.gameObject);
            }

            var spawnBuffer = entityManager.GetBuffer<BarrackSpawnBuffer>(selectedBarracks, true);
            foreach (var spawn in spawnBuffer) {
                var unitQueueItem = Instantiate(unitQueueTemplate, unitQueueContainer);
                unitQueueItem.GetComponent<Image>().sprite = GameConstants.Instance.UnitTypeListSO.GetUnitTypeSO(spawn.Value).sprite;
                unitQueueItem.gameObject.SetActive(true);
            }
        }

        private void Show() {
            gameObject.SetActive(true);
        }

        private void Hide() {
            gameObject.SetActive(false);
        }
    }
}