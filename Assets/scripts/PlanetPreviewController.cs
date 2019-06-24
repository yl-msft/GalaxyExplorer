using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlanetPreviewController : MonoBehaviour
{
    [SerializeField] private Button[] buttons;
    [SerializeField] private Vector3 lightDestinationPosition;
    [SerializeField] private Image selectionImage;

    private GameObject lightObject;
    private Vector3 lightInitialPosition;
    private bool movingLightToDestination;

    private void Start()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
        selectionImage.gameObject.SetActive(false);
    }

    public void OnButtonSelected(int index)
    {
        OnButtonSelected(buttons[index]);
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
            selectionImage.gameObject.SetActive(true);
            selectionImage.transform.SetParent(selectedButton.transform);
            selectionImage.transform.localPosition = Vector3.zero;
        }
        else
        {
            StartCoroutine(MoveLight(false));
            selectionImage.gameObject.SetActive(false);
        }
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        lightObject = null;
        selectionImage.gameObject.SetActive(false);
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
