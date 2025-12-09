using UnityEngine;
using UnityEngine.EventSystems;

public class UIQuitHoldZone : MonoBehaviour,
    IPointerDownHandler, IPointerUpHandler, IPointerExitHandler {

    [Header("Hold Settings")]
    [Tooltip("Seconds the pointer must stay inside to quit.")]
    public float holdDuration = 4f;

    [Tooltip("Optional: require multitouch. Set to 1 to ignore this requirement.")]
    public int requiredTouchCount = 1;

    [Header("Debug")]
    public bool logDebug = false;

    private bool _holding = false;
    private float _holdStartTime;

    // ------------------------------
    // Pointer Events (EventTrigger)
    // ------------------------------

    public void OnPointerDown(PointerEventData eventData) {
        if (!CheckTouchCount())
            return;

        _holding = true;
        _holdStartTime = Time.unscaledTime;

        if (logDebug)
            Debug.Log("[UIQuitHoldZone] Pointer down → start hold");
    }

    public void OnPointerUp(PointerEventData eventData) {
        CancelHold("pointer up");
    }

    public void OnPointerExit(PointerEventData eventData) {
        CancelHold("pointer exited image");
    }

    // ------------------------------
    // Update: check hold duration
    // ------------------------------

    private void Update() {
        if (!_holding)
            return;

        if (!CheckTouchCount()) {
            CancelHold("touch count too low");
            return;
        }

        float held = Time.unscaledTime - _holdStartTime;

        if (held >= holdDuration) {
            if (logDebug)
                Debug.Log("[UIQuitHoldZone] Hold duration reached → quitting");

            QuitApp();
            _holding = false;
        }
    }

    // ------------------------------
    // Helpers
    // ------------------------------

    private bool CheckTouchCount() {
#if UNITY_EDITOR
        return true; // Always allow mouse in editor
#else
        return (requiredTouchCount <= 1) ||
               (Input.touchCount >= requiredTouchCount);
#endif
    }

    private void CancelHold(string reason) {
        if (_holding && logDebug)
            Debug.Log("[UIQuitHoldZone] Hold canceled: " + reason);

        _holding = false;
    }

    private void QuitApp() {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}