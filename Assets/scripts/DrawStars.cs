// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

//using HoloToolkit.Unity;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace GalaxyExplorer
{
    public class RenderProxy : MonoBehaviour
    {
        public DrawStars owner;
        
        private CommandBuffer _commandBuffer;
        
        private bool wasValid;
        private Camera _camera;
        private  const CameraEvent CamEvent = CameraEvent.BeforeForwardAlpha;

        public CommandBuffer CommandBuffer
        {
            set
            {
                _commandBuffer = value;
                _camera.AddCommandBuffer(CamEvent, _commandBuffer);
            }
        }

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            _commandBuffer = new CommandBuffer
            {
                name = "DrawStars Command Buffer"
            };
            _camera.AddCommandBuffer(CamEvent, _commandBuffer);
        }

        private void Update()
        {
            if (owner)
            {
                wasValid = true;
            }
            if (wasValid && !owner)
            {
                Destroy(this);
            }
        }

//#if UNITY_EDITOR
//        /// <summary>
//        /// This will enable seeing the galaxy in the editor view.
//        /// Without that, it will only draw in the game view.
//        /// </summary>
//        private void OnDrawGizmos()
//        {
//            if (owner)
//            {
//                wasValid = true;
//                owner.Render(isEditor: true, _commandBuffer);
//            }
//        }
//#endif
    }

    public class RenderTexturesBucket : SingleInstance<RenderTexturesBucket>
    {
        public RenderTexture downRez;
        public RenderTexture downRezMed;
        public RenderTexture downRezHigh;

        private void CreateBuffers()
        {
            const int downRezFactor = 3;
            downRez = new RenderTexture(Camera.main.pixelWidth >> downRezFactor, Camera.main.pixelHeight >> downRezFactor, 0, RenderTextureFormat.ARGB32);
            downRezMed = new RenderTexture(Camera.main.pixelWidth >> (downRezFactor - 1), Camera.main.pixelHeight >> (downRezFactor - 1), 0, RenderTextureFormat.ARGB32);
            downRezHigh = new RenderTexture(Camera.main.pixelWidth >> (downRezFactor - 2), Camera.main.pixelHeight >> (downRezFactor - 2), 0, RenderTextureFormat.ARGB32);
            downRez.filterMode = FilterMode.Bilinear;
            downRezMed.filterMode = FilterMode.Bilinear;
            downRezHigh.filterMode = FilterMode.Bilinear;
        }

        static bool isInitialized = false;
        public static bool CreateIfNeeded(GameObject owner)
        {
            if (isInitialized) return false;
            
            var go = new GameObject("Galaxy Render Textures");
            go.transform.parent = owner.transform;

            var inst = go.AddComponent<RenderTexturesBucket>();

            inst.CreateBuffers();

            isInitialized = true;
            return true;
        }

        protected override void OnDestroy()
        {
            isInitialized = false;
            base.OnDestroy();
        }
    }

    public class DrawStars : MonoBehaviour
    {
        public float Age;
        public Material starsMaterial;

        public int starCount;

        public SpiralGalaxy galaxy;

        private ComputeBuffer starsData;

        private bool isFirst;

        public Material screenComposeMaterial;
        public Material screenClearMaterial;

        public bool renderIntoDownscaledTarget;
        public MeshRenderer referenceQuad;
        private float originalTransitionAlpha;

        private Mesh cubeMeshProxy;

        private Camera _mainCamera;
        private static readonly int TransitionAlpha = Shader.PropertyToID("_TransitionAlpha");

        private IEnumerator Start()
        {
            while (!Camera.main)
            {
                yield return null;
            }

            _mainCamera = Camera.main;

            var renderProxy = _mainCamera.gameObject.AddComponent<RenderProxy>();
            renderProxy.owner = this;

            if (referenceQuad && referenceQuad.sharedMaterial)
            {
                originalTransitionAlpha = referenceQuad.sharedMaterial.GetFloat(TransitionAlpha);
            }

            renderProxy.CommandBuffer = CreateCommandBuffer();
        }

        public void CreateBuffers(StarVertDescriptor[] stars)
        {
            if (renderIntoDownscaledTarget)
            {
                isFirst = RenderTexturesBucket.CreateIfNeeded(galaxy.gameObject);
            }

            starsData = new ComputeBuffer(stars.Length, StarVertDescriptor.StructSize);
            starsData.SetData(stars);

            var cubeProxyParent = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cubeProxyParent.name = "Cube Proxy";
            cubeProxyParent.transform.parent = transform;
            cubeProxyParent.SetActive(false);

            cubeMeshProxy = cubeProxyParent.GetComponent<MeshFilter>().mesh;

            starCount = stars.Length;
        }

        private static void DisposeBuffer(ref ComputeBuffer buffer)
        {
            if (buffer == null) return;
            buffer.Dispose();
            buffer = null;
        }

        private void OnDestroy()
        {
            if (referenceQuad && referenceQuad.sharedMaterial)
            {
                referenceQuad.sharedMaterial.SetFloat("_TransitionAlpha", originalTransitionAlpha);
            }

            DisposeBuffer(ref starsData);
        }

        private CommandBuffer CreateCommandBuffer()
        {
            var commandBuffer = new CommandBuffer();

            if (renderIntoDownscaledTarget)
            {
                commandBuffer.SetRenderTarget(RenderTexturesBucket.Instance.downRez);
                
                if (isFirst)
                {
                        commandBuffer.Clear();
                }
            }

            commandBuffer.DrawProcedural(galaxy.transform.worldToLocalMatrix, starsMaterial, 0, MeshTopology.Points, starCount, 1);

            if (!renderIntoDownscaledTarget) return commandBuffer;
            commandBuffer.SetRenderTarget(RenderTexture.active);

            if (isFirst) return commandBuffer;
            commandBuffer.Blit(RenderTexturesBucket.Instance.downRez, RenderTexturesBucket.Instance.downRezMed, screenComposeMaterial, 0);
            commandBuffer.Blit(RenderTexturesBucket.Instance.downRezMed, RenderTexturesBucket.Instance.downRezHigh, screenComposeMaterial, 0);

//                    var renderCubeScale = galaxy.transform.lossyScale;
//                    renderCubeScale.x *= galaxy.MaxEllipseScale * 2 * Math.Max(galaxy.XRadii, galaxy.ZRadii);
//                    renderCubeScale.z *= galaxy.MaxEllipseScale * 2 * Math.Max(galaxy.XRadii, galaxy.ZRadii);
//                    renderCubeScale.y *= galaxy.YRange * 4 * Mathf.Lerp(2, .5f, camDir.y);

//#if (UNITY_EDITOR)
//                    if (isEditor)
//                    {
//                        // true if called from DrawGizmos...
//                        screenComposeMaterial.SetPass(2);
//                    }
//                    else
//                    {
//                        screenComposeMaterial.SetPass(1);
//                    }
//#else
//                    screenComposeMaterial.SetPass(1);
//#endif
//                    commandBuffer.DrawMesh(cubeMeshProxy, Matrix4x4.TRS(galaxy.transform.position, galaxy.transform.rotation, renderCubeScale));
//                    Graphics.DrawMeshNow(cubeMeshProxy, Matrix4x4.TRS(galaxy.transform.position, galaxy.transform.rotation, renderCubeScale));

            return commandBuffer;
        }

        private void Update()
        {
            if (!enabled || !galaxy.gameObject.activeInHierarchy)
            {
                return;
            }

            var mainCamTransform = _mainCamera.transform;

            if (renderIntoDownscaledTarget)
            {

                if (referenceQuad)
                {
                    referenceQuad.sharedMaterial.SetFloat("_TransitionAlpha", galaxy.TransitionAlpha);
                }
            }

            float wsScale = galaxy.worldSpaceScale * galaxy.transform.lossyScale.x;

            var camDir = galaxy.transform.InverseTransformPoint(mainCamTransform.position).normalized;

            if (!renderIntoDownscaledTarget)
            {
                wsScale *= Mathf.Clamp01(4 * Math.Max(.1f, Mathf.Abs(camDir.y * .1f)));
            }
            else if (galaxy.isShadow)
            {
                var scaleMultiplier = 1 - Mathf.Clamp01(4 * Math.Max(.1f, Mathf.Abs(camDir.y * .1f)));
                wsScale *= scaleMultiplier;
            }

            // Draw the galaxy
            starsMaterial.SetBuffer("_Stars", starsData);
            starsMaterial.SetVector("_LocalCamDir", camDir);
            starsMaterial.SetFloat("_WSScale", wsScale);

            starsMaterial.SetVector("_Color", galaxy.tint * galaxy.tintMult * Mathf.Lerp(galaxy.verticalTintMultiplier.x, galaxy.verticalTintMultiplier.y, Mathf.Abs(camDir.y)));

            starsMaterial.SetVector("_EllipseSize", new Vector4(galaxy.XRadii, galaxy.ZRadii, galaxy.MinEllipseScale, galaxy.MaxEllipseScale));
            starsMaterial.SetVector("_FuzzySideScale", galaxy.FuzzySideScale);
            starsMaterial.SetVector("_CamPos", mainCamTransform.position);
            starsMaterial.SetVector("_CamForward", mainCamTransform.forward);
            starsMaterial.SetFloat("_Age", Age);

            starsMaterial.SetMatrix("_GalaxyWorld", galaxy.transform.localToWorldMatrix);

            starsMaterial.SetFloat("_TransitionAlpha", galaxy.TransitionAlpha);

            if (renderIntoDownscaledTarget)
            {

                if (!isFirst)
                {

                    var renderCubeScale = galaxy.transform.lossyScale;
                    renderCubeScale.x *= galaxy.MaxEllipseScale * 2 * Math.Max(galaxy.XRadii, galaxy.ZRadii);
                    renderCubeScale.z *= galaxy.MaxEllipseScale * 2 * Math.Max(galaxy.XRadii, galaxy.ZRadii);
                    renderCubeScale.y *= galaxy.YRange * 4 * Mathf.Lerp(2, .5f, camDir.y);

//                    screenComposeMaterial.mainTexture = RenderTexturesBucket.Instance.downRezHigh;
//#if (UNITY_EDITOR)
//                    if (isEditor)
//                    {
//                        // true if called from DrawGizmos...
//                        screenComposeMaterial.SetPass(2);
//                    }
//                    else
//                    {
//                        screenComposeMaterial.SetPass(1);
//                    }
//#else
//                    screenComposeMaterial.SetPass(1);
//#endif
//                    commandBuffer.DrawMesh(cubeMeshProxy, Matrix4x4.TRS(galaxy.transform.position, galaxy.transform.rotation, renderCubeScale));
//                    Graphics.DrawMeshNow(cubeMeshProxy, Matrix4x4.TRS(galaxy.transform.position, galaxy.transform.rotation, renderCubeScale));
                }
            }
        }
    }
}