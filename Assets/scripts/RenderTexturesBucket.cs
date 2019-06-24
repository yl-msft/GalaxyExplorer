using UnityEngine;

namespace GalaxyExplorer
{
    public class RenderTexturesBucket : SingleInstance<RenderTexturesBucket>
    {
        public RenderTexture downRez;
        public RenderTexture downRezMed;
        public RenderTexture downRezHigh;

        public MeshRenderer LowRezTarget, MedRezTarget, HighRezTarget;
        private static readonly int MainTex = Shader.PropertyToID("_MainTex");

        private void CreateBuffers()
        {
            const int downRezFactor = 3;
            downRez = new RenderTexture(Camera.main.pixelWidth >> downRezFactor, Camera.main.pixelHeight >> downRezFactor, 0, RenderTextureFormat.ARGB32);
            downRezMed = new RenderTexture(Camera.main.pixelWidth >> (downRezFactor - 1), Camera.main.pixelHeight >> (downRezFactor - 1), 0, RenderTextureFormat.ARGB32);
            downRezHigh = new RenderTexture(Camera.main.pixelWidth >> (downRezFactor - 2), Camera.main.pixelHeight >> (downRezFactor - 2), 0, RenderTextureFormat.ARGB32);
            downRez.filterMode = FilterMode.Bilinear;
            downRezMed.filterMode = FilterMode.Bilinear;
            downRezHigh.filterMode = FilterMode.Bilinear;



            if (LowRezTarget != null)
            {
                LowRezTarget.material.SetTexture(MainTex, downRez);
            }

            if (MedRezTarget != null)
            {
                MedRezTarget.material.SetTexture(MainTex, downRezMed);
            }

            if (HighRezTarget != null)
            {
                HighRezTarget.material.SetTexture(MainTex, downRezHigh);
            }
        }
        
        private void SetTestTargets(){}

        static bool isInitialized = false;

        public static bool CreateIfNeeded(GameObject owner)
        {
            if (isInitialized) return false;

            if (Instance == null)
            {
                var go = new GameObject("Galaxy Render Textures");
                go.transform.parent = owner.transform;

                go.AddComponent<RenderTexturesBucket>();
            }

            Instance.CreateBuffers();

            isInitialized = true;
            return true;
        }

        protected override void OnDestroy()
        {
            isInitialized = false;
            base.OnDestroy();
        }
    }
}