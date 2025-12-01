using UnityEngine;

/// <summary>
/// Hidden touch/mouse zone to quit the application.
/// Place on any GameObject in the scene (e.g. a global manager).
/// </summary>
public class HiddenQuitZone : MonoBehaviour
{
    [Header("Zone (normalized screen coords)")]
    [Tooltip("X,Y = bottom-left corner, W,H = size, all in 0-1 range.")]
    public Rect normalizedZone = new Rect(0.9f, 0.9f, 0.1f, 0.1f); // top-right 10%

    [Header("Hold settings")]
    [Tooltip("Seconds user must hold inside the zone to trigger quit.")]
    public float holdDuration = 4f;

    [Tooltip("Optional extra safety: require at least this many touches.\n" +
             "Set to 1 to disable multi-touch requirement.")]
    public int requiredTouchCount = 1;

    [Header("Debug")]
    public bool logDebug = false;

    bool _isHolding;
    float _holdStartTime;

    void Update()
    {
#if UNITY_EDITOR
        // In editor we usually use mouse; treat it as single-touch.
        HandlePointer(Input.GetMouseButton(0),
                      Input.GetMouseButtonDown(0),
                      Input.GetMouseButtonUp(0),
                      Input.mousePosition,
                      1);
#else
        // In build: support both touch and mouse.
        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);
            bool pressed   = t.phase == TouchPhase.Began;
            bool held      = t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary;
            bool released  = t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled;

            HandlePointer(held, pressed, released, t.position, Input.touchCount);
        }
        else
        {
            // Fallback to mouse in case someone uses a regular PC.
            HandlePointer(Input.GetMouseButton(0),
                          Input.GetMouseButtonDown(0),
                          Input.GetMouseButtonUp(0),
                          Input.mousePosition,
                          1);
        }
#endif
    }

    void HandlePointer(bool isDown, bool justPressed, bool justReleased, Vector3 screenPos, int currentTouchCount)
    {
        if (!isDown)
        {
            // Reset when pointer not held
            _isHolding = false;
            return;
        }

        if (requiredTouchCount > 1 && currentTouchCount < requiredTouchCount)
        {
            // Not enough touches to count as a quit gesture
            _isHolding = false;
            return;
        }

        // Convert screen position to normalized 0-1
        float nx = screenPos.x / Screen.width;
        float ny = screenPos.y / Screen.height;
        Vector2 normPos = new Vector2(nx, ny);

        bool insideZone = normalizedZone.Contains(normPos);

        if (justPressed)
        {
            if (insideZone)
            {
                _isHolding = true;
                _holdStartTime = Time.unscaledTime;
                if (logDebug) Debug.Log("[HiddenQuitZone] Hold started in zone.");
            }
            else
            {
                _isHolding = false;
            }
        }

        if (_isHolding)
        {
            if (!insideZone)
            {
                // Pointer left the zone → cancel
                _isHolding = false;
                if (logDebug) Debug.Log("[HiddenQuitZone] Left zone, cancel hold.");
                return;
            }

            float heldFor = Time.unscaledTime - _holdStartTime;
            if (heldFor >= holdDuration)
            {
                if (logDebug) Debug.Log("[HiddenQuitZone] Hold time reached, quitting app.");
                QuitApp();
                _isHolding = false;
            }
        }

        if (justReleased)
        {
            _isHolding = false;
        }
    }

    void QuitApp()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

#if UNITY_EDITOR
    // Optional: draw the zone in Scene view for debugging (not visible in build)
    void OnDrawGizmosSelected()
    {
        // approximate the zone in world-space using camera
        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 bl = cam.ScreenToWorldPoint(new Vector3(
            normalizedZone.x * Screen.width,
            normalizedZone.y * Screen.height,
            cam.nearClipPlane));
        Vector3 tr = cam.ScreenToWorldPoint(new Vector3(
            (normalizedZone.x + normalizedZone.width) * Screen.width,
            (normalizedZone.y + normalizedZone.height) * Screen.height,
            cam.nearClipPlane));

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube((bl + tr) * 0.5f, new Vector3(Mathf.Abs(tr.x - bl.x), Mathf.Abs(tr.y - bl.y), 0.01f));
    }
#endif
}