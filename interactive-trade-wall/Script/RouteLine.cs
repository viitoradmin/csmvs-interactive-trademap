using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class RouteLine : MonoBehaviour
{
    [Header("Control Points (in world space order)")]
    public Transform[] controlPoints;

    [Header("Line")]
    public LineRenderer line;
    [Range(8, 256)] public int samplesPerSegment = 32;
    public float lineWidth = 0.12f;

    [Header("Flow Animation")]
    public float scrollSpeed = 0.25f;   // UV units/sec
    public float tilingPerUnit = 2.0f;  // how many dots per world unit

    Material _matInstance;
    float _routeLength;

    void Reset()
    {
        line = GetComponent<LineRenderer>();
        if (!line) line = gameObject.AddComponent<LineRenderer>();
    }

    void OnEnable()
    {
        if (line && line.sharedMaterial)
            _matInstance = Instantiate(line.sharedMaterial);
        if (line) line.material = _matInstance;
    }

    void Update()
    {
        if (controlPoints == null || controlPoints.Length < 2 || line == null) return;

        var points = BuildCatmullRom(controlPoints, samplesPerSegment);
        line.positionCount = points.Count;
        line.SetPositions(points.ToArray());
        line.widthMultiplier = lineWidth;

        _routeLength = ApproximateLength(points);
        // Tile texture relative to length so dot density stays constant
        var scale = new Vector2(_routeLength * tilingPerUnit, 1f);
        if (_matInstance) _matInstance.mainTextureScale = scale;

        // Scroll to create motion
        if (_matInstance)
        {
            var offs = _matInstance.mainTextureOffset;
            offs.x = (offs.x - Time.deltaTime * scrollSpeed) % 1f;
            _matInstance.mainTextureOffset = offs;
        }
    }

    static List<Vector3> BuildCatmullRom(Transform[] cps, int samples)
    {
        var pts = new List<Vector3>();
        if (cps.Length == 2) // simple lerp if only 2
        {
            for (int i = 0; i <= samples; i++)
            {
                float t = i / (float)samples;
                pts.Add(Vector3.Lerp(cps[0].position, cps[1].position, t));
            }
            return pts;
        }

        // Catmull-Rom through all points (closed = false)
        for (int i = 0; i < cps.Length - 1; i++)
        {
            Vector3 p0 = i == 0 ? cps[i].position : cps[i - 1].position;
            Vector3 p1 = cps[i].position;
            Vector3 p2 = cps[i + 1].position;
            Vector3 p3 = (i + 2 < cps.Length) ? cps[i + 2].position : cps[i + 1].position;

            for (int s = 0; s < samples; s++)
            {
                float t = s / (float)samples;
                pts.Add(CatmullRom(p0, p1, p2, p3, t));
            }
        }
        pts.Add(cps[cps.Length - 1].position);
        return pts;
    }

    static Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        // 0.5 * ((2 * P1) + (-P0 + P2) * t + (2P0 - 5P1 + 4P2 - P3) * t^2 + (-P0 + 3P1 - 3P2 + P3) * t^3)
        float t2 = t * t, t3 = t2 * t;
        return 0.5f * ((2f * p1) +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
            (-p0 + 3f * p1 - 3f * p2 + p3) * t3);
    }

    static float ApproximateLength(List<Vector3> pts)
    {
        float len = 0f;
        for (int i = 1; i < pts.Count; i++) len += Vector3.Distance(pts[i-1], pts[i]);
        return len;
    }
}