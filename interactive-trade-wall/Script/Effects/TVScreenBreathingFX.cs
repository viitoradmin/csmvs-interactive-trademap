using UnityEngine;
using DG.Tweening;

/// <summary>
/// Gentle "breathing" scale pulse for a map-content Transform (e.g. "TVScreenParent (1)"),
/// driven by DOTween (no per-frame Update).
///
/// <para>Scales the whole content group up and down so the map appears to breathe, without
/// touching the camera. Attach to a plain Transform that parents the map visuals.</para>
///
/// <para>Objects listed in <see cref="excludeAffectedObjects"/> are reparented OUT of the
/// target while breathing (to the target's parent), so the scale never affects them, and
/// restored to their original parent when breathing stops.</para>
/// </summary>
[DisallowMultipleComponent]
public class TVScreenBreathingFX : MonoBehaviour
{
    [Tooltip("Transform to scale. Defaults to this GameObject's Transform.")]
    public Transform target;

    [Tooltip("How much bigger the content grows on inhale, as a fraction. 0.04 = 4%.")]
    [Range(0f, 0.3f)] public float scaleAmplitude = 0.04f;

    [Tooltip("Seconds for one full inhale + exhale cycle. Lower = faster breathing.")]
    public float cycleDuration = 5f;

    [Tooltip("If false, the script does not scale the target at all.")]
    public bool BreathingEnabled = true;

    [Tooltip("Objects to EXCLUDE from the breathing scale. While breathing they are reparented " +
             "out to the target's parent (so they don't scale) and restored to their original " +
             "parent when breathing stops.")]
    public Transform[] excludeAffectedObjects;

    private Vector3     _baseScale;
    private Vector3     _basePosition;
    private Transform[] _excludeOriginalParents;
    private bool        _excluded;
    private Tween       _tween;

    private void Awake()
    {
        if (target == null) target = transform;
        _baseScale    = target.localScale;
        _basePosition = target.position;
        CacheExcludedOriginalParents();
    }

    /// <summary>Captures each excluded object's authored parent (as of scene start).</summary>
    private void CacheExcludedOriginalParents()
    {
        if (excludeAffectedObjects == null) { _excludeOriginalParents = null; return; }

        _excludeOriginalParents = new Transform[excludeAffectedObjects.Length];
        for (int i = 0; i < excludeAffectedObjects.Length; i++)
            if (excludeAffectedObjects[i] != null)
                _excludeOriginalParents[i] = excludeAffectedObjects[i].parent;
    }

    private void OnEnable()
    {
        if (BreathingEnabled) StartBreathing();
    }

    private void OnDisable()
    {
        StopBreathing();
    }

    /// <summary>(Re)starts the breathing tween from the authored baseline.</summary>
    public void StartBreathing()
    {
        if (target == null) target = transform;

        StopBreathing();
        BreathingEnabled = true;

        target.localScale = _baseScale;

        // Move excluded objects out of the scaling target so the pulse never touches them.
        DetachExcluded();

        float half = Mathf.Max(0.01f, cycleDuration * 0.5f);
        float amt  = Mathf.Max(0f, scaleAmplitude);

        _tween = target.DOScale(_baseScale * (1f + amt), half)
                       .SetEase(Ease.InOutSine)
                       .SetLoops(-1, LoopType.Yoyo)
                       .SetUpdate(true)      // independent of Time.timeScale
                       .SetLink(gameObject); // auto-kill if destroyed
    }

    /// <summary>Kills the tween, snaps back to baseline, and restores excluded objects.</summary>
    public void StopBreathing()
    {
        if (_tween != null)
        {
            _tween.Kill();
            _tween = null;
        }

        if (target != null)
            target.localScale = _baseScale;

        RestoreExcluded();
    }

    /// <summary>Reparents excluded objects to the target's parent (out of the scaled group).</summary>
    private void DetachExcluded()
    {
        if (_excluded || excludeAffectedObjects == null) return;

        Transform holder = target != null ? target.parent : null;
        for (int i = 0; i < excludeAffectedObjects.Length; i++)
            if (excludeAffectedObjects[i] != null)
                excludeAffectedObjects[i].SetParent(holder, worldPositionStays: true);

        _excluded = true;
    }

    /// <summary>Reparents excluded objects back to their original (authored) parent.</summary>
    private void RestoreExcluded()
    {
        if (!_excluded || excludeAffectedObjects == null || _excludeOriginalParents == null) return;

        for (int i = 0; i < excludeAffectedObjects.Length; i++)
            if (excludeAffectedObjects[i] != null)
                excludeAffectedObjects[i].SetParent(_excludeOriginalParents[i], worldPositionStays: true);

        _excluded = false;
    }

    /// <summary>Enable breathing at runtime. Re-syncs the base position to the current pose.</summary>
    public void EnableBreathing()
    {
        // Set the current position as the new base (keeps position handling intact).
        _basePosition = target != null ? target.position : transform.position;
        StartBreathing();
    }

    /// <summary>Disable breathing and snap back to baseline. Re-syncs the base position.</summary>
    public void DisableBreathing()
    {
        BreathingEnabled = false;
        // Sync base to current so re-enable is seamless and position is never lost.
        _basePosition = target != null ? target.position : transform.position;
        StopBreathing();
    }
}
