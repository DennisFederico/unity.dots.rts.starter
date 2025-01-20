using System;
using rts.mono;
using UnityEngine;

namespace rts.UI {
    public class GameOverUI : MonoBehaviour {
        private void Start() {
            DOTSEventManager.Instance.OnSoldiersHQDestroyed += OnGameOver;
            Hide();
        }

        private void OnGameOver(object sender, EventArgs e) {
            Show();
            Time.timeScale = 0.25f;
        }

        public void Show() {
            gameObject.SetActive(true);
        }
        
        public void Hide() {
            gameObject.SetActive(false);
        }
    }
}