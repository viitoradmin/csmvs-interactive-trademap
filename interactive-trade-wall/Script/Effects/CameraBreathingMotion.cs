using UnityEngine;

/// <summary>
/// Very subtle breathing motion for an orthographic camera.
/// You can enable/disable the motion at runtime to avoid conflicts
/// with other camera effects (focus pans, zooms, etc.).
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraBreathingMotion : MonoBehaviour
{
    [Header("Amplitude (world units)")]
    [Tooltip("Max horizontal offset from the base position.")]
    public float amplitudeX = 0.25f;

    [Tooltip("Max vertical offset from the base position.")]
    public float amplitudeY = 0.15f;

    [Header("Timing (seconds per full loop)")]
    [Tooltip("Duration of one horizontal cycle.")]
    public float periodX = 40f;

    [Tooltip("Duration of one vertical cycle.")]
    public float periodY = 55f;

    [Header("Phase offset to avoid mechanical feel")]
    public float phaseOffsetX = 0.0f;
    public float phaseOffsetY = 0.8f;

    [Header("Noise jitter (optional, tiny)")]
    public float noiseMagnitude = 0.03f;
    public float noiseFrequency = 0.05f;

    [Header("Control")]
    [Tooltip("If false, script does not move the camera at all.")]
    public bool BreathingEnabled = true;

    // Internal state
    Vector3 _basePosition;
    float _noiseSeedX;
    float _noiseSeedY;
    bool _initialized;

    void Awake()
    {
        Initialize();
    }

    void OnEnable()
    {
        Initialize();
    }

    void Initialize()
    {
        _basePosition = transform.position;
        _noiseSeedX = Random.Range(0f, 1000f);
        _noiseSeedY = Random.Range(0f, 1000f);
        _initialized = true;
    }

    void Update()
    {
        if (!_initialized) Initialize();

        if (!BreathingEnabled)
        {
            // When breathing is off, we don't touch the camera.
            // Keep base position synced to current position so
            // when we re-enable, breathing starts from the new spot.
            _basePosition = transform.position;
            return;
        }

        float t = Time.time;

        float x = 0f;
        float y = 0f;

        if (periodX > 0.01f)
        {
            float tx = (t + phaseOffsetX) / periodX * Mathf.PI * 2f;
            x = Mathf.Sin(tx) * amplitudeX;
        }

        if (periodY > 0.01f)
        {
            float ty = (t + phaseOffsetY) / periodY * Mathf.PI * 2f;
            y = Mathf.Sin(ty) * amplitudeY;
        }

        // Optional organic jitter using Perlin noise (very subtle)
        float nx = 0f;
        float ny = 0f;
        if (noiseMagnitude > 0f)
        {
            nx = (Mathf.PerlinNoise(_noiseSeedX, t * noiseFrequency) - 0.5f) * 2f * noiseMagnitude;
            ny = (Mathf.PerlinNoise(_noiseSeedY, t * noiseFrequency) - 0.5f) * 2f * noiseMagnitude;
        }

        transform.position = new Vector3(
            _basePosition.x + x + nx,
            _basePosition.y + y + ny,
            _basePosition.z
        );
    }

    /// <summary>
    /// Enable breathing from the camera's *current* position.
    /// </summary>
    public void EnableBreathing()
    {
        // Set the current position as the new base.
        _basePosition = transform.position;
        BreathingEnabled = true;
    }

    /// <summary>
    /// Disable breathing. After this, the script stops modifying
    /// the camera position and other effects are free to move it.
    /// </summary>
    public void DisableBreathing()
    {
        BreathingEnabled = false;
        // Optional: sync base to current so re-enable is seamless
        _basePosition = transform.position;
    }
}