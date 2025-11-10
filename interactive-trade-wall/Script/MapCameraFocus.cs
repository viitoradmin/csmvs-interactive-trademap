using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// Smoothly pans & zooms an orthographic camera to frame a target region on a 2D map.
/// Author: ChatGPT (Unity 2021+ / URP/BRP)
/// </summary>
[RequireComponent(typeof(Camera))]
public class MapCameraFocus : MonoBehaviour
{
    [Header("References")]
    [Tooltip("SpriteRenderer of the full map (used for clamping the camera).")]
    public SpriteRenderer mapSprite;

    [Header("Motion")]
    [Tooltip("Seconds to complete a focus animation.")]
    public float duration = 1.2f;
    [Tooltip("Easing curve for the animation time.")]
    public AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [Tooltip("Extra world-units padding around the target region.")]
    public float padding = 0.5f;

    [Header("Zoom Limits")]
    [Tooltip("Minimum and maximum orthographicSize allowed.")]
    public Vector2 orthoLimits = new Vector2(1.0f, 12.0f);

    Camera cam;
    Coroutine anim;

    void Awake()
    {
        cam = GetComponent<Camera>();
        cam.orthographic = true;
    }

    /// <summary>
    /// Focus to a BoxCollider2D region (nice for authoring regions in the scene).
    /// </summary>
    public void FocusOn(BoxCollider2D box) => FocusOn(box.bounds);

    /// <summary>
    /// Focus to a Bounds region (center + size in world units).
    /// </summary>
    public void FocusOn(Bounds region) => StartFocus(region.center, region.size);

    /// <summary>
    /// Focus to two world points (min/max corners).
    /// </summary>
    public void FocusOn(Vector2 worldMin, Vector2 worldMax)
    {
        var center = (worldMin + worldMax) * 0.5f;
        var size = (Vector2)(worldMax - worldMin);
        StartFocus(center, new Vector2(Mathf.Abs(size.x), Mathf.Abs(size.y)));
    }

    /// <summary>
    /// Reset to show the whole map.
    /// </summary>
    public void FocusFullMap()
    {
        if (!mapSprite) return;
        FocusOn(mapSprite.bounds);
    }

    // ---------------- internal ----------------

    void StartFocus(Vector2 targetCenter, Vector2 targetSize, Action onComplete = null)
    {
        if (anim != null) StopCoroutine(anim);

        // Compute target ortho size to fit the rect with padding
        var padded = targetSize + Vector2.one * (padding * 2f);
        float aspect = cam.aspect;
        float sizeByHeight = padded.y * 0.5f;
        float sizeByWidth  = (padded.x * 0.5f) / aspect;
        float targetOrtho  = Mathf.Max(sizeByHeight, sizeByWidth);

        targetOrtho = Mathf.Clamp(targetOrtho, orthoLimits.x, orthoLimits.y);

        // Clamp target center so final view stays inside map bounds
        if (mapSprite)
        {
            var mapB = mapSprite.bounds;
            float extY = targetOrtho;
            float extX = targetOrtho * aspect;

            float cx = Mathf.Clamp(targetCenter.x, mapB.min.x + extX, mapB.max.x - extX);
            float cy = Mathf.Clamp(targetCenter.y, mapB.min.y + extY, mapB.max.y - extY);
            targetCenter = new Vector2(cx, cy);
        }

        anim = StartCoroutine(AnimateTo(targetCenter, targetOrtho,onComplete));
    }

    IEnumerator AnimateTo(Vector2 targetPos, float targetOrtho, Action OnComplete) 
    {
        Vector3 startPos = transform.position;
        float startOr = cam.orthographicSize;

        // Keep camera’s Z
        Vector3 endPos = new Vector3(targetPos.x, targetPos.y, startPos.z);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.0001f, duration);
            float k = ease.Evaluate(Mathf.Clamp01(t));

            transform.position = Vector3.Lerp(startPos, endPos, k);
            cam.orthographicSize = Mathf.Lerp(startOr, targetOrtho, k);

            yield return null;
        }

        transform.position = endPos;
        cam.orthographicSize = targetOrtho;
        anim = null;

        OnComplete?.Invoke();
    }

    public void MovetoActual(Vector2 pos,float orthoSize, Action onComplete = null)
    {
        Debug.Log("<color=red>Camera pos should be:  </color>"+pos);
        StartCoroutine(AnimateTo(pos, orthoSize, onComplete));
    }
}