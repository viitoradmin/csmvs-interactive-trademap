using System.Collections;
using UnityEngine;

/// <summary>
/// Controls highlight/intensity of a single route line sprite.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class RouteLineHighlight : MonoBehaviour
{
    [Header("Colors")]
    public Color idleColor  = new Color(1f, 0.9f, 0.8f, 0.5f);
    public Color activeColor = new Color(1f, 0.98f, 0.92f, 0.95f);

    [Header("Thickness (Y scale)")]
    public float idleThickness  = 1.0f;
    public float activeThickness = 1.15f;

    [Header("Transition")]
    public float fadeDuration = 0.6f;

    SpriteRenderer _sr;
    Vector3 _baseScale;
    Coroutine _fadeRoutine;

    void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        _baseScale = transform.localScale;
        // Start in idle state
        _sr.color = idleColor;
        SetThickness(idleThickness);
    }

    void SetThickness(float factor)
    {
        transform.localScale = new Vector3(_baseScale.x, _baseScale.y * factor, _baseScale.z);
    }

    public void SetActive(bool active)
    {
        if (_fadeRoutine != null)
            StopCoroutine(_fadeRoutine);

        _fadeRoutine = StartCoroutine(Co_Fade(active));
    }

    IEnumerator Co_Fade(bool active)
    {
        Color startColor = _sr.color;
        Color targetColor = active ? activeColor : idleColor;

        float startThickness = transform.localScale.y / _baseScale.y;
        float targetThickness = active ? activeThickness : idleThickness;

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float n = Mathf.SmoothStep(0f, 1f, t / fadeDuration);

            _sr.color = Color.Lerp(startColor, targetColor, n);
            float thickness = Mathf.Lerp(startThickness, targetThickness, n);
            SetThickness(thickness);

            yield return null;
        }

        _sr.color = targetColor;
        SetThickness(targetThickness);
        _fadeRoutine = null;
    }
}