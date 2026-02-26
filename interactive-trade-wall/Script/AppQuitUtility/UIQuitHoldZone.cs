using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIQuitHoldZone : MonoBehaviour,
    IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [Header("Hold Settings")] [Tooltip("Seconds the pointer must stay inside to quit.")]
    public float holdDuration = 4f;

    [Tooltip("Optional: require multitouch. Set to 1 to ignore this requirement.")]
    public int requiredTouchCount = 1;

    [Header("Debug")] public bool logDebug = false;

    private bool _holding = false;
    private float _holdStartTime;

    [SerializeField] private TextMeshProUGUI appVersionText;
    [SerializeField] private RectTransform confirmationPanel;
    [SerializeField] private Button appQuitButton;
    [SerializeField] private Button dataClearButton;
    [SerializeField] private Button closePanelButton;
    [SerializeField] private RectTransform quitZoneButtonElement;
    
    [SerializeField] private APIHandler _apiHandler;

    private APIHandler apiHandler{
        get{
            if (_apiHandler == null){
                _apiHandler = FindObjectOfType<APIHandler>();
            }

            return _apiHandler;
        }
        set{ _apiHandler = value; }
    }

    [SerializeField] private MediaManager _mediaManager;

    private MediaManager mediaManager{
        get{
            if (_mediaManager == null){
                _mediaManager = FindObjectOfType<MediaManager>();
            }

            return _mediaManager;
        }
    }

    private void Awake(){
        appQuitButton.onClick.RemoveAllListeners();
        appQuitButton.onClick.AddListener(OnClickQuitApp);

        dataClearButton.onClick.RemoveAllListeners();
        dataClearButton.onClick.AddListener(OnClickRemoveData);

        closePanelButton.onClick.RemoveAllListeners();
        closePanelButton.onClick.AddListener(() => {
            SetConfirmationPanelActive(false);
        });
        SetConfirmationPanelActive(false);
    }

    private void SetConfirmationPanelActive(bool active){
        confirmationPanel.gameObject.SetActive(active);
        quitZoneButtonElement.gameObject.SetActive(!active);
    }
    
    private void Start(){
        appVersionText.text = $"v_{Application.version}";
    }
    // ------------------------------
    // Pointer Events (EventTrigger)
    // ------------------------------

    public void OnPointerDown(PointerEventData eventData){
        if (!CheckTouchCount())
            return;

        _holding = true;
        _holdStartTime = Time.unscaledTime;

        if (logDebug)
            Debug.Log("[UIQuitHoldZone] Pointer down → start hold");
    }

    public void OnPointerUp(PointerEventData eventData){
        CancelHold("pointer up");
    }

    public void OnPointerExit(PointerEventData eventData){
        CancelHold("pointer exited image");
    }

    // ------------------------------
    // Update: check hold duration
    // ------------------------------

    private void Update(){
        if (!_holding)
            return;

        if (!CheckTouchCount()){
            CancelHold("touch count too low");
            return;
        }

        float held = Time.unscaledTime - _holdStartTime;

        if (held >= holdDuration){
            if (logDebug)
                Debug.Log("[UIQuitHoldZone] Hold duration reached → quitting");

            _holding = false;
            SetConfirmationPanelActive(true);
        }
    }

    // ------------------------------
    // Helpers
    // ------------------------------

    private bool CheckTouchCount(){
#if UNITY_EDITOR
        return true; // Always allow mouse in editor
#else
        return (requiredTouchCount <= 1) ||
               (Input.touchCount >= requiredTouchCount);
#endif
    }

    private void CancelHold(string reason){
        if (_holding && logDebug)
            Debug.Log("[UIQuitHoldZone] Hold canceled: " + reason);

        _holding = false;
    }

    internal void OnClickQuitApp(){
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        Debug.Log("[LOG]: Quitting app");
    }

    private void OnClickRemoveData(){
        ClearData();
        OnClickQuitApp();
    }
    private void ClearData(){
        Debug.Log("[LOG]: Clearing local data and Quitting");
        if (apiHandler != null) {
            apiHandler.ClearLocalStoredData();
        }
        if (mediaManager != null) {
            mediaManager.ClearLocalStoredData();
        }   
    }
}