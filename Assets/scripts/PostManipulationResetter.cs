using System.Collections;
using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;

[RequireComponent(typeof(ManipulationHandler))]
public class PostManipulationResetter : MonoBehaviour
{
    private ManipulationHandler _handler;
    private Vector3 _startPos, _startScale;
    private Quaternion _startRotation;
    private Coroutine _coroutine;
    private Transform _transformTarget;

    public bool ResetTranslate, ResetRotate, ResetScale;
    public AnimationCurve ResetCurve;

    public float ResetDuration = 2f;

    private void Awake()
    {
        _handler = GetComponent<ManipulationHandler>();
        _transformTarget = _handler.HostTransform;
        _handler.OnManipulationStarted.AddListener(OnManipulationStart);
        _handler.OnManipulationEnded.AddListener(OnManipulationEnd);
        StoreTargets();
    }

    private IEnumerator ResetCoroutine()
    {
        if (!(ResetTranslate || ResetScale || ResetRotate))
        {
            yield break;
        }
            
        var time = 0f;
        var tr = _transformTarget;
        var fromPos = tr.position;
        var fromScale = tr.localScale;
        var fromRot = tr.rotation;
        while (time <= ResetDuration)
        {
            var a = time / ResetDuration;
            var v = ResetCurve.Evaluate(a);
            
            if (ResetTranslate)
            {
                tr.position = Vector3.Lerp(fromPos, _startPos, v);
            }

            if (ResetScale)
            {
                tr.localScale = Vector3.Lerp(fromScale, _startScale, v);
            }

            if (ResetRotate)
            {
                tr.rotation = Quaternion.Lerp(fromRot, _startRotation, v);
            }

            time += Time.deltaTime;
            
            yield return null;
        }

        if (ResetTranslate)
        {
            tr.position = _startPos;
        }

        if (ResetScale)
        {
            tr.localScale = _startScale;
        }

        if (ResetRotate)
        {
            tr.rotation = _startRotation;
        }
    }

    private void OnManipulationStart(ManipulationEventData e)
    {
        if (_coroutine == null) return;
        StopCoroutine(_coroutine);
        _coroutine = null;
    }

    private void OnManipulationEnd(ManipulationEventData e)
    {
        _coroutine = StartCoroutine(ResetCoroutine());
    }

    private void StoreTargets()
    {
        var tr = _transformTarget;
        _startPos = tr.position;
        _startScale = tr.localScale;
        _startRotation = tr.rotation;
    }

    public void Reset()
    {
        if (_coroutine != null)
        {
            return;
        }
        _coroutine = StartCoroutine(ResetCoroutine());
    }
}
