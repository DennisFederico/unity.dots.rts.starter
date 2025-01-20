using System;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace rts.mono {
    public class DOTSEventManager : MonoBehaviour {
        
        public static DOTSEventManager Instance { get; private set; }
        
        public event EventHandler OnBarracksQueueChanged;
        public event EventHandler OnSoldiersHQDestroyed;
        
        private void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
        public void TriggerOnBarracksQueueChanged(NativeList<Entity> barracksEntitiesThatChangedList) {
            foreach(Entity entity in barracksEntitiesThatChangedList) {
                OnBarracksQueueChanged?.Invoke(entity, EventArgs.Empty);    
            }
        }
        
        public void TriggerOnSoldiersHQDestroyed() {
            OnSoldiersHQDestroyed?.Invoke(this, EventArgs.Empty);
        }
    }
}