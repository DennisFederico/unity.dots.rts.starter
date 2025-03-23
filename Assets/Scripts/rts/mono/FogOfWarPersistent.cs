using System;
using UnityEngine;

namespace rts.mono {
    public class FogOfWarPersistent : MonoBehaviour {

        [SerializeField] private RenderTexture fogOfWarSourceTexture;
        [SerializeField] private RenderTexture fogOfWarDestinationTexture;
        [SerializeField] private RenderTexture fogOfWarDestinationTextureCache;
        [SerializeField] private Material multiplyFogOfWarMaterial;

        private void Start() {
            Graphics.Blit(fogOfWarSourceTexture, fogOfWarDestinationTextureCache);
            Graphics.Blit(fogOfWarSourceTexture, fogOfWarDestinationTexture);
        }

        private void Update() {
            Graphics.Blit(fogOfWarSourceTexture, fogOfWarDestinationTextureCache, multiplyFogOfWarMaterial, 0);
            Graphics.Blit(fogOfWarDestinationTextureCache, fogOfWarDestinationTexture);
        }
    }
}