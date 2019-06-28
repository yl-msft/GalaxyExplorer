using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class POIContent : MonoBehaviour
{
    [SerializeField] private Transform poiText;
    [SerializeField] private Transform textTargetLocation;
    [SerializeField] private MonoBehaviour objectToRunCoroutine;
    [SerializeField] private float textAnimationDelay = .25f;

    private Transform originalTextParent;
    private Vector3 originalTextLocalPosition;
    private bool hiding;

    public Coroutine ShowContents()
    {
        return objectToRunCoroutine.StartCoroutine(ShowRoutine());
    }

    public Coroutine HideContents()
    {
        hiding = true;
        return objectToRunCoroutine.StartCoroutine(HideRoutine());
    }

    private IEnumerator ShowRoutine()
    {
        originalTextParent = poiText.parent;
        originalTextLocalPosition = poiText.localPosition;
        yield return new WaitForSeconds(textAnimationDelay);
        var startPosition = poiText.position;
        var startRotation = poiText.rotation;
        
        var overTime = .5f;
        var timeSoFar = 0f;
        while (timeSoFar < overTime && !hiding)
        {
            poiText.position = Vector3.Slerp(startPosition, textTargetLocation.position, timeSoFar/overTime);
            poiText.rotation = Quaternion.Slerp(startRotation, textTargetLocation.rotation, timeSoFar/overTime);
            yield return null;
            timeSoFar += Time.deltaTime;
        }

        if (hiding)
        {
            yield break;
        }
        poiText.SetParent(textTargetLocation, true);
        poiText.position = textTargetLocation.position;
        poiText.localRotation = Quaternion.identity;
    }

    private IEnumerator HideRoutine()
    {
        poiText.SetParent(originalTextParent, true);
        var startPosition = poiText.localPosition;
        var startRotation = poiText.localRotation;
        var overTime = .5f;
        var timeSoFar = 0f;
        while (timeSoFar < overTime)
        {
            poiText.localPosition = Vector3.Slerp(startPosition, originalTextLocalPosition, timeSoFar/overTime);
            poiText.localRotation = Quaternion.Slerp(startRotation, Quaternion.identity, timeSoFar/overTime);
            yield return null;
            timeSoFar += Time.deltaTime;
        }
        poiText.localPosition = originalTextLocalPosition;
        poiText.localRotation = Quaternion.identity;
        hiding = false;
    }
}
