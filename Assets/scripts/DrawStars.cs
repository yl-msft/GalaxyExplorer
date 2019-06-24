// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

//using HoloToolkit.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering;

namespace GalaxyExplorer
{
    public class DrawStars : MonoBehaviour
    {
        private float originalTransitionAlpha;

        private RenderTargetIdentifier _downRezId, _medRezId, _highRezId;
        
        private Camera _mainCamera;
        private readonly Dictionary<Camera, CommandBuffer> _cameraToCommandBuffer = new Dictionary<Camera, CommandBuffer>();
        
        private ComputeBuffer starsData;
        private bool isFirst;
        
        
        [SerializeField]
        private CameraEvent cameraEvent = CameraEvent.BeforeForwardOpaque;

        public float Age;
        public Material starsMaterial;

        public int starCount;

        public SpiralGalaxy galaxy;

        public Material screenComposeMaterial;

        public bool renderIntoDownscaledTarget;
        public MeshRenderer referenceQuad;
        
        private static readonly int Stars = Shader.PropertyToID("_Stars");
        private static readonly int LocalCamDir = Shader.PropertyToID("_LocalCamDir");
        private static readonly int WsScale = Shader.PropertyToID("_WSScale");
        private static readonly int PColor = Shader.PropertyToID("_Color");
        private static readonly int EllipseSize = Shader.PropertyToID("_EllipseSize");
        private static readonly int FuzzySideScale = Shader.PropertyToID("_FuzzySideScale");
        private static readonly int CamPos = Shader.PropertyToID("_CamPos");
        private static readonly int CamForward = Shader.PropertyToID("_CamForward");
        private static readonly int PAge = Shader.PropertyToID("_Age");
        private static readonly int TransitionAlpha = Shader.PropertyToID("_TransitionAlpha");

        private IEnumerator Start()
        {
            while (!Camera.main)
            {
                yield return null;
            }

            _mainCamera = Camera.main;

            if (referenceQuad && referenceQuad.sharedMaterial)
            {
                originalTransitionAlpha = referenceQuad.sharedMaterial.GetFloat(TransitionAlpha);
            }
            
            starsMaterial.SetBuffer(Stars, starsData);
        }

        public void CreateBuffers(StarVertDescriptor[] stars)
        {
            if (renderIntoDownscaledTarget)
            {
                isFirst = RenderTexturesBucket.CreateIfNeeded(galaxy.gameObject);
            }

            _downRezId = new RenderTargetIdentifier(RenderTexturesBucket.Instance.downRez);
            _medRezId = new RenderTargetIdentifier(RenderTexturesBucket.Instance.downRezMed);
            _highRezId = new RenderTargetIdentifier(RenderTexturesBucket.Instance.downRezHigh);
            
            starsData = new ComputeBuffer(stars.Length, StarVertDescriptor.StructSize);
            starsData.SetData(stars);
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

        private void OnDisable()
        {
            foreach (var camera in _cameraToCommandBuffer)
            {
                if (camera.Key)
                {
                    camera.Key.RemoveCommandBuffer(cameraEvent, camera.Value);
                }
            }
        }

        private void OnDrawGizmos()
        {
            UpdateCamera(true);
        }

        private void UpdateCamera(bool isSceneView = false)
        {
            var cam = Camera.current;
            if(cam == null) return;
            if (!_cameraToCommandBuffer.TryGetValue(cam, out var cb))
            {
                cb = new CommandBuffer();
                _cameraToCommandBuffer.Add(cam, cb);
                cam.AddCommandBuffer(cameraEvent, cb);
            }
            else
            {
                cb.Clear();
            }
            UpdateCommandBuffer(cb, isSceneView);
        }

        private void UpdateCommandBuffer(CommandBuffer commandBuffer, bool isSceneView = false)
        {
            if (renderIntoDownscaledTarget)
            {
                commandBuffer.SetRenderTarget(_downRezId);
                
                if (isFirst)
                {
                        commandBuffer.ClearRenderTarget(true, true, Color.clear);
                }
            }

            commandBuffer.DrawProcedural(galaxy.transform.localToWorldMatrix, starsMaterial, 0, MeshTopology.Points, starCount);

            if (!renderIntoDownscaledTarget) return;
            commandBuffer.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);

            if (isFirst) return;
            commandBuffer.SetGlobalTexture("_MainTex", _downRezId);
            commandBuffer.Blit(_downRezId, _medRezId, screenComposeMaterial, 0);
            commandBuffer.SetGlobalTexture("_MainTex", _medRezId);
            commandBuffer.Blit(_medRezId, _highRezId, screenComposeMaterial, 0);
            commandBuffer.SetGlobalTexture("_MainTex", _highRezId);
            commandBuffer.Blit(_highRezId, BuiltinRenderTextureType.CameraTarget);
        }

        private void Update()
        {
            if (!enabled || !galaxy.gameObject.activeInHierarchy)
            {
                OnDisable();
                return;
            }

            var mainCamTransform = _mainCamera.transform;

            if (renderIntoDownscaledTarget)
            {

                if (referenceQuad)
                {
                    referenceQuad.sharedMaterial.SetFloat(TransitionAlpha, galaxy.TransitionAlpha);
                }
            }

            var wsScale = galaxy.worldSpaceScale * galaxy.transform.lossyScale.x;

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

            starsMaterial.SetVector(LocalCamDir, camDir);
            starsMaterial.SetFloat(WsScale, wsScale);

            starsMaterial.SetVector(PColor, galaxy.tint * galaxy.tintMult * Mathf.Lerp(galaxy.verticalTintMultiplier.x, galaxy.verticalTintMultiplier.y, Mathf.Abs(camDir.y)));

            starsMaterial.SetVector(EllipseSize, new Vector4(galaxy.XRadii, galaxy.ZRadii, galaxy.MinEllipseScale, galaxy.MaxEllipseScale));
            starsMaterial.SetVector(FuzzySideScale, galaxy.FuzzySideScale);
            starsMaterial.SetVector(CamPos, mainCamTransform.position);
            starsMaterial.SetVector(CamForward, mainCamTransform.forward);
            starsMaterial.SetFloat(PAge, Age);

            starsMaterial.SetFloat(TransitionAlpha, galaxy.TransitionAlpha);

            UpdateCamera();
        }
    }
}