using UnityEngine;

/// <summary>
/// Ambient breathing glow for civilization regions.
/// Attach to a Glow_* GameObject that has a SpriteRenderer.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class AmbientCivilizationGlow : MonoBehaviour
{
    [Header("Breath Timing")]
    [Tooltip("Duration of one full glow cycle (seconds).")]
    public float cycleDuration = 10f;   // slow museum pacing

    [Tooltip("Random phase offset so all glows are not in sync.")]
    public float randomPhaseOffsetRange = 3f;

    [Header("Alpha")]
    [Range(0f, 1f)] public float minAlpha = 0.15f;
    [Range(0f, 1f)] public float maxAlpha = 0.45f;

    [Header("Scale")]
    [Tooltip("Scale multiplier at minimum glow.")]
    public float minScale = 1.0f;
    [Tooltip("Scale multiplier at maximum glow.")]
    public float maxScale = 1.08f;

    [Header("Color")]
    [Tooltip("Base tint color (no alpha).")]
    public Color baseColor = new Color(1f, 0.86f, 0.6f, 1f); // warm parchment

    SpriteRenderer _sr;
    MaterialPropertyBlock _mpb;

    float _phaseOffset;

    void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        _mpb = new MaterialPropertyBlock();

        // Each instance gets a slightly different phase so they don't glow in sync
        _phaseOffset = Random.Range(-randomPhaseOffsetRange, randomPhaseOffsetRange);
    }

    void Update()
    {
        if (cycleDuration <= 0.01f) return;

        // 0..1 over time, plus per-instance offset
        float t = (Time.time + _phaseOffset) / cycleDuration;
        float sin = Mathf.Sin(t * Mathf.PI * 2f); // -1..1
        float normalized = (sin + 1f) * 0.5f;     // 0..1

        // Smoothstep to avoid harsh peaks
        normalized = Mathf.SmoothStep(0f, 1f, normalized);

        // Compute alpha & scale
        float alpha = Mathf.Lerp(minAlpha, maxAlpha, normalized);
        float scale = Mathf.Lerp(minScale, maxScale, normalized);

        // Apply scale (local)
        transform.localScale = new Vector3(scale, scale, 1f);

        // Apply color via MaterialPropertyBlock
        Color c = baseColor;
        c.a = alpha;

        _sr.GetPropertyBlock(_mpb);
        _mpb.SetColor("_Color", c);          // works with Sprites/Default & many sprite shaders
        _sr.SetPropertyBlock(_mpb);
    }
}