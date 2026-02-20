using UnityEngine;

public class HiddenWipeData : MonoBehaviour
{
    [Header("Zone (normalized screen coords)")] [Tooltip("X,Y = bottom-left corner, W,H = size, all in 0-1 range.")]
    public Rect normalizedZone = new Rect(0.9f, 0.9f, 0.1f, 0.1f); // top-right 10%

    [Header("Hold settings")] [Tooltip("Seconds user must hold inside the zone to trigger quit.")]
    public float holdDuration = 4f;

    [Tooltip("Optional extra safety: require at least this many touches.\n" +
             "Set to 1 to disable multi-touch requirement.")]
    public int requiredTouchCount = 1;

    [Header("Debug")] public bool logDebug = false;

    bool _isHolding;
    float _holdStartTime;
    private Vector2 _lastNormPos;
    private int _lastTouchCount;
    [SerializeField]
    private APIHandler _apiHandler;

    private APIHandler apiHandler
    {
        get
        {
            if (_apiHandler == null)
            {
                _apiHandler = FindObjectOfType<APIHandler>();
            }

            return _apiHandler;
        }
        set { _apiHandler = value; }
    }

    [SerializeField]
    private MediaManager _mediaManager;

    private MediaManager mediaManager
    {
        get
        {
            if (_mediaManager == null)
            {
                _mediaManager = FindObjectOfType<MediaManager>();
            }

            return _mediaManager;
        }
    }
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
            bool pressed = t.phase == TouchPhase.Began;
            bool held = t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary;
            bool released = t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled;

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

        // Store for debug display
        _lastNormPos = normPos;
        _lastTouchCount = currentTouchCount;

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
                RemoveLocalData();
                QuitApp();
                _isHolding = false;
            }
        }

        if (justReleased)
        {
            _isHolding = false;
        }
    }

    private void RemoveLocalData()
    {
        if (apiHandler != null) {
            //apiHandler.ClearLocalStoredData();
            Debug.Log("//apiHandler.ClearLocalStoredData();");
        }
        if (mediaManager != null) {
            //mediaManager.ClearLocalStoredData();
            Debug.Log("//mediaManager.ClearLocalStoredData();");
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
    void OnGUI()
    {
        if (!logDebug) return;

        // Bottom-right debug box
        float boxWidth = 350f;
        float boxHeight = 180f;
        float margin = 10f;

        Rect boxRect = new Rect(
            Screen.width - boxWidth - margin,
            Screen.height - boxHeight - margin,
            boxWidth,
            boxHeight
        );

        // Semi-transparent background
        GUI.color = new Color(0, 0, 0, 0.8f);
        GUI.Box(boxRect, "");
        GUI.color = Color.white;

        // Content
        GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.fontSize = 14;
        labelStyle.normal.textColor = Color.white;
        labelStyle.padding = new RectOffset(10, 10, 5, 5);

        string debugInfo = $"<b>Hidden Wipe Data - Debug</b>\n\n";
        debugInfo += $"Touch Count: {_lastTouchCount} (Required: {requiredTouchCount})\n";
        debugInfo += $"Position: ({_lastNormPos.x:F3}, {_lastNormPos.y:F3})\n";
        debugInfo += $"Zone: ({normalizedZone.x:F2}, {normalizedZone.y:F2}) [{normalizedZone.width:F2}×{normalizedZone.height:F2}]\n";
        debugInfo += $"Inside Zone: {normalizedZone.Contains(_lastNormPos)}\n";
        debugInfo += $"Holding: {_isHolding}\n";

        if (_isHolding)
        {
            float elapsed = Time.unscaledTime - _holdStartTime;
            float progress = Mathf.Clamp01(elapsed / holdDuration);
            debugInfo += $"Progress: {elapsed:F2}s / {holdDuration:F1}s ({progress * 100:F0}%)";
        }

        GUI.Label(boxRect, debugInfo, labelStyle);
    }
#endif
    
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