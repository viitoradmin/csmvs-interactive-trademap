using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class RoutePointPulse : MonoBehaviour
{
    [Header("Pulse Settings")]
    public float minScale = 0.8f;
    public float maxScale = 1.2f;
    public float pulseSpeed = 1.5f;
    public float fadeStrength = 0.4f;

    [Header("Color")]
    public Color baseColor = new Color(1f, 0.9f, 0.3f, 1f); // warm yellow

    private SpriteRenderer sr;
    private float timeOffset;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        // Randomize offset so all points aren't synced
        timeOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    void Update()
    {
        float t = (Mathf.Sin(Time.time * pulseSpeed + timeOffset) + 1f) * 0.5f;
        float scale = Mathf.Lerp(minScale, maxScale, t);
        transform.localScale = Vector3.one * scale;

        // Subtle fade in/out of brightness
        float brightness = Mathf.Lerp(1f - fadeStrength, 1f, t);
        sr.color = baseColor * brightness;
    }
}