using UnityEngine;

/// <summary>
/// Parallax movement for 2D orthographic scenes.
/// Each layer moves relative to camera motion to create depth.
/// </summary>
[ExecuteAlways]
public class ParallaxLayer : MonoBehaviour
{
    [Tooltip("Reference camera (typically the main camera).")]
    public Camera targetCamera;

    [Tooltip("How much this layer resists camera movement.\n" +
             "0 = moves exactly with camera (no parallax)\n" +
             "1 = stays fixed while camera moves (infinite background)\n" +
             "<0 or >1 exaggerates parallax.")]
    [Range(-1f, 2f)]
    public float parallaxStrength = 0.5f;

    [Tooltip("Optional: lock Z so artists can move layer in Z for sorting.")]
    public bool lockZ = true;

    Vector3 _basePosition;
    Vector3 _baseCamPosition;
    bool _initialized;

    void OnEnable()
    {
        Init();
    }

    void Init()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        if (targetCamera == null) return;

        _basePosition = transform.position;
        _baseCamPosition = targetCamera.transform.position;
        _initialized = true;
    }

    void LateUpdate()
    {
        if (!Application.isPlaying)
        {
            // Keep bases synced if camera or layer is moved in editor
            _initialized = false;
        }

        if (!_initialized) Init();
        if (targetCamera == null) return;

        Vector3 camDelta = targetCamera.transform.position - _baseCamPosition;

        // Parallax: layer offset opposite to camera motion
        Vector3 offset = camDelta * (1f - parallaxStrength);

        Vector3 newPos = _basePosition + new Vector3(offset.x, offset.y, 0f);
        if (lockZ) newPos.z = _basePosition.z;

        transform.position = newPos;
    }
}