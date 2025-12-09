using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Ink-write reveal + subtle halo breathing for a city label.
/// Works with a SpriteRenderer halo and a TextMeshPro text.
/// </summary>
public class CityLabelEffect : MonoBehaviour
{
    [Header("References")]
    public TextMeshPro labelText;
    public SpriteRenderer haloRenderer;

    [Header("Reveal Timing")]
    public float writeDuration = 1.2f;   // time to "write" the whole word
    public float settleScale = 1.05f;    // overshoot scale
    public float settleDuration = 0.25f;

    [Header("Idle / Active Alpha")]
    [Range(0f, 1f)] public float activeTextAlpha = 1f;
    [Range(0f, 1f)] public float idleTextAlpha = 0.4f;
    [Range(0f, 1f)] public float activeHaloAlpha = 0.45f;
    [Range(0f, 1f)] public float idleHaloAlpha = 0.15f;

    [Header("Halo Breathing")]
    public float haloBreathAmplitude = 0.06f; // scale delta
    public float haloBreathPeriod = 4f;

    // state
    Coroutine _revealRoutine;
    bool _isActive;
    Color _baseTextColor;
    Color _baseHaloColor;
    Vector3 _baseScale;

    void Awake()
    {
        if (labelText == null || haloRenderer == null)
        {
            Debug.LogWarning($"CityLabelEffect missing references on {name}");
            enabled = false;
            return;
        }

        _baseScale = transform.localScale;
        _baseTextColor = labelText.color;
        _baseHaloColor = haloRenderer.color;

        // start in idle (low alpha)
        SetAlpha(idleTextAlpha, idleHaloAlpha);
    }

    void Update()
    {
        // subtle breathing on halo only when active
        // if (_isActive && haloBreathPeriod > 0.01f && haloBreathAmplitude > 0f)
        // {
        //     float t = Time.time / haloBreathPeriod * Mathf.PI * 2f;
        //     float s = 2f + Mathf.Sin(t) * haloBreathAmplitude;
        //     haloRenderer.transform.localScale = new Vector3(s, s, 1f);
        // }
    }

    void SetAlpha(float textA, float haloA)
    {
        Color tc = _baseTextColor;
        tc.a = textA;
        labelText.color = tc;

        Color hc = _baseHaloColor;
        hc.a = haloA;
        haloRenderer.color = hc;
    }

    /// <summary>
    /// Play ink-write reveal and set label to active state.
    /// </summary>
    public void PlayReveal()
    {
        if (_revealRoutine != null)
            StopCoroutine(_revealRoutine);

        _revealRoutine = StartCoroutine(Co_Reveal());
    }

    IEnumerator Co_Reveal()
    {
        _isActive = true;

        // Reset base state
        transform.localScale = _baseScale;
        SetAlpha(0f, 0f);
        labelText.maxVisibleCharacters = 0;

        // Ensure text is ready
        labelText.ForceMeshUpdate();
        int totalChars = labelText.textInfo.characterCount;
        if (totalChars == 0)
            yield break;

        // Ink write: reveal characters over time
        float t = 0f;
        while (t < writeDuration)
        {
            t += Time.deltaTime;
            float normalized = Mathf.Clamp01(t / writeDuration);
            int visible = Mathf.RoundToInt(Mathf.Lerp(0, totalChars, normalized));
            labelText.maxVisibleCharacters = visible;

            // Fade up alpha together
            float a = Mathf.SmoothStep(0f, activeTextAlpha, normalized);
            float ha = Mathf.SmoothStep(0f, activeHaloAlpha, normalized);
            SetAlpha(a, ha);

            yield return null;
        }

        labelText.maxVisibleCharacters = totalChars;
        SetAlpha(activeTextAlpha, activeHaloAlpha);

        // Small settle scale overshoot
        Vector3 startScale = _baseScale * settleScale;
        Vector3 endScale = _baseScale;
        transform.localScale = startScale;

        float st = 0f;
        while (st < settleDuration)
        {
            st += Time.deltaTime;
            float n = Mathf.SmoothStep(0f, 1f, st / settleDuration);
            transform.localScale = Vector3.Lerp(startScale, endScale, n);
            yield return null;
        }

        transform.localScale = endScale;
        _revealRoutine = null;
    }

    /// <summary>
    /// Fade label to idle (de-emphasized but still visible).
    /// </summary>
    public void SetIdle()
    {
        _isActive = false;
        haloRenderer.transform.localScale = Vector3.one; // reset breath scale

        if (_revealRoutine != null)
            StopCoroutine(_revealRoutine);

        StartCoroutine(Co_FadeToIdle());
    }

    IEnumerator Co_FadeToIdle()
    {
        float startTextA = labelText.color.a;
        float startHaloA = haloRenderer.color.a;

        float dur = 0.6f;
        float t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            float n = Mathf.Clamp01(t / dur);
            float ta = Mathf.Lerp(startTextA, idleTextAlpha, n);
            float ha = Mathf.Lerp(startHaloA, idleHaloAlpha, n);
            SetAlpha(ta, ha);
            yield return null;
        }

        SetAlpha(idleTextAlpha, idleHaloAlpha);
    }
}