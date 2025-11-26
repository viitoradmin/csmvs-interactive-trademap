using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Creates a flowing "caravan" effect along a row of dot sprites.
/// Attach this to DotsRoot. Children must be SpriteRenderers.
/// </summary>
public class RouteCaravanFlow : MonoBehaviour
{
    [Header("Flow settings")]
    [Tooltip("Seconds for a wave to travel from first dot to last.")]
    public float travelDuration = 4f;

    [Tooltip("How many dots are strongly lit at once (0-1 over total count).")]
    [Range(0.05f, 1f)]
    public float activeWindow = 0.25f;

    [Header("Visuals")]
    [Range(0f, 1f)] public float idleAlpha = 0.15f;
    [Range(0f, 1f)] public float activeAlpha = 0.9f;
    public float idleScale = 1.0f;
    public float activeScale = 1.3f;

    [Tooltip("If true, flow goes from last dot to first.")]
    public bool reverse = false;

    [Header("Control")]
    public bool flowEnabled = true;

    readonly List<SpriteRenderer> _dots = new List<SpriteRenderer>();
    readonly List<float> _dotPositions = new List<float>(); // normalized 0..1 along sequence

    void Awake()
    {
        _dots.Clear();
        _dotPositions.Clear();

        // Get children in order in hierarchy
        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            var sr = child.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                _dots.Add(sr);
            }
        }

        int count = _dots.Count;
        if (count == 0) return;

        // Positions evenly spaced 0..1
        for (int i = 0; i < count; i++)
        {
            float p = (count == 1) ? 0.5f : (float)i / (count - 1);
            _dotPositions.Add(p);

            // Initialize to idle state
            SetDotVisual(i, idleAlpha, idleScale);
        }
    }

    void SetDotVisual(int index, float alpha, float scale)
    {
        var sr = _dots[index];
        Color c = sr.color;
        c.a = alpha;
        sr.color = c;

        sr.transform.localScale = Vector3.one * scale;
    }

    void Update()
    {
        if (!flowEnabled || _dots.Count == 0 || travelDuration <= 0.01f)
            return;

        float t = (Time.time % travelDuration) / travelDuration; // 0..1 over a cycle
        if (reverse) t = 1f - t;

        float halfWindow = activeWindow * 0.5f;

        for (int i = 0; i < _dots.Count; i++)
        {
            float p = _dotPositions[i];

            // Distance along route between current wave center and this dot
            float delta = Mathf.DeltaAngle(t * 360f, p * 360f) / 360f;
            float ad = Mathf.Abs(delta); // 0..0.5

            // Map into 0..1 within active window
            float n = Mathf.InverseLerp(halfWindow, 0f, ad); // 0 outside, 1 at center
            n = Mathf.Clamp01(n);

            // Smooth falloff
            float intensity = Mathf.SmoothStep(0f, 1f, n);

            float alpha = Mathf.Lerp(idleAlpha, activeAlpha, intensity);
            float scale = Mathf.Lerp(idleScale, activeScale, intensity);

            SetDotVisual(i, alpha, scale);
        }
    }

    // External control
    public void EnableFlow()
    {
        flowEnabled = true;
    }

    public void DisableFlow(bool fadeToIdle = true)
    {
        flowEnabled = false;
        if (fadeToIdle)
        {
            for (int i = 0; i < _dots.Count; i++)
                SetDotVisual(i, idleAlpha, idleScale);
        }
    }
}