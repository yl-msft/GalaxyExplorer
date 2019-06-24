using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlanetPreviewController : MonoBehaviour
{
    [SerializeField] private Button[] buttons;
    [SerializeField] private Vector3 lightDestinationPosition;

    private GameObject lightObject;
    private Vector3 lightInitialPosition;
    private bool movingLightToDestination;

    private void Start()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    public void OnButtonSelected(Button selectedButton)
    {
        foreach (var button in buttons)
        {
            button.interactable = button != selectedButton;
        }

        if (lightObject == null)
        {
            lightObject = GameObject.Find("LightSourcePosition");
            if (lightObject != null)
            {
                lightInitialPosition = lightObject.transform.position;
            }
        }

        if (selectedButton != null)
        {
            if (!movingLightToDestination)
            {
                StartCoroutine(MoveLight(true));
            }
        }
        else
        {
            StartCoroutine(MoveLight(false));
        }
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        lightObject = null;
    }

    private IEnumerator MoveLight(bool toDestination)
    {
        if (lightObject != null)
        {
            if (toDestination)
            {
                lightObject.transform.position = lightDestinationPosition;
                movingLightToDestination = true;
            }
            else
            {
                lightObject.transform.position = lightInitialPosition;
                movingLightToDestination = false;
            }
        }
        yield return null;
    }
}
