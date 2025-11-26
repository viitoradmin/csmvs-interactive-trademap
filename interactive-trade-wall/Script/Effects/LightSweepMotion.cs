using UnityEngine;

public class LightSweepMotion : MonoBehaviour
{
    [Tooltip("Seconds for the sweep to go from left to right and back.")]
    public float cycleDuration = 60f;   // nice and slow

    [Tooltip("If true, sweep moves vertically (bottom-top-bottom).")]
    public bool vertical = false;

    Material _mat;
    int _bandCenterID;
    int _verticalID;

    void Awake()
    {
        var sr = GetComponent<SpriteRenderer>();
        _mat = sr.material;  // instance per renderer
        _bandCenterID = Shader.PropertyToID("_BandCenter");
        _verticalID   = Shader.PropertyToID("_Vertical");
    }

    void Start()
    {
        if (_mat != null)
            _mat.SetFloat(_verticalID, vertical ? 1f : 0f);
    }

    void Update()
    {
        if (_mat == null || cycleDuration <= 0.01f)
            return;

        float t = Mathf.PingPong(Time.time / cycleDuration, 1f); // 0..1..0

        // Extend slightly beyond edges so sweep fully enters/leaves
        float bandCenter = Mathf.Lerp(-0.2f, 1.2f, t);

        _mat.SetFloat(_bandCenterID, bandCenter);
    }
}