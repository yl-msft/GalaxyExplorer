using System.Collections;
using GalaxyExplorer;
using Microsoft.MixedReality.Toolkit;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class UiWorldPreview : MonoBehaviour
{
    private static int MAX_LAYER_NUMBER = 9;
    
    [SerializeField] private int targetSlotId;
    [SerializeField] private RawImage image;
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI displayNameArea;
    [SerializeField] private PlanetPreviewController planetPreviewController;

    private UiPreviewTarget target;
    private Camera targetCamera;
    private RenderTexture renderTexture;

    private static int layerNumber;

    private void OnEnable()
    {
        if (!GalaxyExplorerManager.IsDesktop)
        {
            return;
        }
        displayNameArea.gameObject.SetActive(false);
        image.enabled = false;
        StartCoroutine(WaitForTarget());
    }

    void Initialize()
    {
        button.onClick.AddListener(HandleClick);
        var cameraObject = new GameObject("UIViewCamera");
        targetCamera = cameraObject.AddComponent<Camera>();
        renderTexture = new RenderTexture(256,256, 24, RenderTextureFormat.ARGB32);
        renderTexture.antiAliasing = 2;
        targetCamera.targetTexture = renderTexture;
        image.texture = renderTexture;
        displayNameArea.gameObject.SetActive(true);
        image.enabled = true;
        displayNameArea.text = target.displayName;
        
        var targetLayer = LayerMask.NameToLayer($"PreviewLayer{layerNumber}");
        target.gameObject.SetLayerRecursively(targetLayer);
        targetCamera.cullingMask = 1 << targetLayer;
        layerNumber++;
        if (layerNumber > MAX_LAYER_NUMBER)
        {
            layerNumber = 0;
        }
        
        targetCamera.transform.SetParent(target.transform);
        PositionCamera();
        targetCamera.clearFlags = CameraClearFlags.Color;
        targetCamera.backgroundColor = Color.clear;
        targetCamera.nearClipPlane = .0001f;
    }

    private void PositionCamera()
    {
        targetCamera.transform.localPosition = target.initialPosition;
        targetCamera.transform.localRotation = target.initialRotation;
        targetCamera.fieldOfView = target.initialFov;
    }

    IEnumerator WaitForTarget()
    {
        var waitForOneSecond = new WaitForSeconds(1);
        target = GetTargetById(targetSlotId);
        while (target == null)
        {
            yield return waitForOneSecond;
            target = GetTargetById(targetSlotId);
        }
        
        Initialize();
    }

    private UiPreviewTarget GetTargetById(int slotId)
    {
        var previewTargets = FindObjectsOfType<UiPreviewTarget>();
        foreach (var previewTarget in previewTargets)
        {
            if (previewTarget.slotId == slotId)
            {
                return previewTarget;
            }
        }
        return null;
    }

    private void HandleClick()
    {
        if (planetPreviewController != null)
        {
            planetPreviewController.OnButtonSelected(button);
        }
        target.forceSolver.OnPointerDown();
    }

    private void OnDisable()
    {
        if (targetCamera != null)
        {
            Destroy(targetCamera.gameObject);
        }
        button.onClick.RemoveAllListeners();
    }

    private void OnDestroy()
    {
        if (renderTexture != null)
        {
            renderTexture.Release();
        }
    }
}
