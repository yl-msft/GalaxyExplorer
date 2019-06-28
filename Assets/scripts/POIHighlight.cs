using System.Collections;
using UnityEngine;

public class POIHighlight : MonoBehaviour
{
    [SerializeField]
    private float _totalDuration = 1f;

    [SerializeField]
    private float _delay = 0f;

    private float _startOffset = -1f;
    private float _endOffset = 1f;
    private float _textureOffsetY = 0f;

    private float _fadeInEndPercentage = 0.2f;
    private float _fadeOutStartPercentage = 0.5f;

    private Material _material;

    private void Awake()
    {
        _material = GetComponent<Renderer>().material;
        Vector2 textureOffset = _material.GetTextureOffset("_MainTex");
        _textureOffsetY = textureOffset.y;
    }

    private void OnEnable()
    {
        if (_material == null)
        {
            _material = GetComponent<Renderer>().material;
        }

        StartLerpTextureOffset();
    }

    private void OnDestroy()
    {
        SetAlphaOnMaterial(1f);
        _material.SetTextureOffset("_MainTex", Vector2.zero);
    }

    public void StartLerpTextureOffset()
    {
        StartCoroutine(LerpTextureOffset());
    }

    private IEnumerator LerpTextureOffset()
    {
        // Reset all values
        Vector2 newOffset = new Vector2(_startOffset, _textureOffsetY);
        _material.SetTextureOffset("_MainTex", newOffset);

        float alpha = 0f;
        SetAlphaOnMaterial(alpha);

        float timer = 0f;

        yield return new WaitForSeconds(_delay);

        while (timer < _totalDuration)
        {
            // Normalize the current time passed
            float currentPercentage = Mathf.Lerp(0f, 1f, timer / _totalDuration);

            // Set the texture offset
            newOffset.x = Mathf.Lerp(_endOffset, _startOffset, currentPercentage);
            _material.SetTextureOffset("_MainTex", newOffset);

            // Set the alpha on the material
            alpha = GetNewAlpha(timer, currentPercentage);
            SetAlphaOnMaterial(alpha);

            timer += Time.deltaTime;

            yield return null;
        }
    }

    private float GetNewAlpha(float timer, float currentPercentage)
    {
        float alpha = 0f;

        if (currentPercentage < _fadeInEndPercentage)
        {
            alpha = Mathf.Lerp(0f, 1f, currentPercentage / _fadeInEndPercentage);
        }
        else if (currentPercentage < _fadeOutStartPercentage)
        {
            alpha = 1;
        }
        else
        {
            alpha = Mathf.Lerp(1f, 0f, ((currentPercentage - _fadeOutStartPercentage) / (1f - _fadeOutStartPercentage)) * 1f / _fadeOutStartPercentage);
        }

        return alpha;
    }

    private void SetAlphaOnMaterial(float alpha)
    {
        _material.color = new Color(_material.color.r, _material.color.b, _material.color.g, alpha);
    }
}