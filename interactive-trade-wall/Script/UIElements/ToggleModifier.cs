using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class ToggleModifier : MonoBehaviour {

    private Toggle _toggle;

    [SerializeField] private bool _changeTextColorOnActive;
    [SerializeField] private bool _changeFontsOnActive;

    [SerializeField] private RawImage _textureImage;
    [SerializeField] private TMP_Text _text;
    [SerializeField] private Color _activeTextColor = Color.white;
    [SerializeField] private Color _inactiveTextColor = Color.black;

    [SerializeField] private Image toogleGraphicImage;
    [SerializeField] private Sprite _activeToggleImage;
    [SerializeField] private Sprite _inactiveToggleSprite;

    [SerializeField] private TMP_FontAsset _activeTextFonts;
    [SerializeField] private TMP_FontAsset _inactiveTextFonts;

    public UnityEvent<bool> _onToggleValueChange;
    public UnityEvent _onToggleValueTrue;

    public int _id; //1 = Import ; 2 = Export
    private void Awake() {
        _toggle = GetComponent<Toggle>();
        _toggle.onValueChanged.AddListener(OnToggleValueChange);
    }
    private void OnToggleValueChange(bool value) {
        SetDecorations(value);
        //_onToggleValueChange.Invoke(value);
        if (value) {
            Debug.Log("Toggle true " + gameObject.name);
            _onToggleValueTrue.Invoke();
            
        }
    }

    public void SetDecorations(bool value) {
        if (_changeTextColorOnActive) {
            _text.color = value ? _activeTextColor : _inactiveTextColor;
        }
        // if (_changeFontsOnActive)
        // {
        //     _text.font = value ? _activeTextFonts : _inactiveTextFonts;
        // }
        toogleGraphicImage.sprite = value ? _activeToggleImage : _inactiveToggleSprite;
        ConnectionManager.Instance.RaiseEventForRouteClick(_id, value?true:false);
    }

    public void SetupTextureToggle(Texture displayImage, UnityAction onClickButtonAction) {
        SetToggleValues(onClickButtonAction);
        _textureImage.texture = displayImage;
    }

    public void SetToggleValues(UnityAction onClickButtonAction) {
        _toggle = GetComponent<Toggle>();
        _toggle.onValueChanged.RemoveAllListeners();
        _onToggleValueTrue.RemoveAllListeners();
        _toggle.onValueChanged.AddListener(OnToggleValueChange);
        _onToggleValueTrue.AddListener(onClickButtonAction);
        _toggle.group = GetComponentInParent<ToggleGroup>();
    }
}
