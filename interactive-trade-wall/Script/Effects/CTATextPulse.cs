using UnityEngine;
using UnityEngine.UI;

public class CTATextPulse : MonoBehaviour
{
    public Graphic textGraphic;    // Text or TMP_Text (both inherit Graphic)
    public float pulsePeriod = 3.5f;
    public float alphaMin = 0.5f;
    public float alphaMax = 1f;
    public float scaleAmplitude = 0.03f;

    Vector3 baseScale;
    Color baseColor;

    void Awake()
    {
        if (textGraphic == null) textGraphic = GetComponent<Graphic>();
        baseScale = transform.localScale;
        baseColor = textGraphic.color;
    }

    void Update()
    {
        float t = Time.time / Mathf.Max(0.01f, pulsePeriod);
        float wave = (Mathf.Sin(t * Mathf.PI * 2f) + 1f) * 0.5f; // 0..1

        float a = Mathf.Lerp(alphaMin, alphaMax, wave);
        Color c = baseColor;
        c.a = a;
        textGraphic.color = c;

        float s = 1f + (wave - 0.5f) * 2f * scaleAmplitude;
        transform.localScale = baseScale * s;
    }
}