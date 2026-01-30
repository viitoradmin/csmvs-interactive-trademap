using System;
using System.Collections.Generic;
using TMPro; // Covers both TextMeshPro and TextMeshProUGUI
using UnityEngine;
using UnityEngine.UI; // Covers Legacy Text

[Serializable]
public class LanguageTextPair {
    public Language language;
    [TextArea] public string message; // Added TextArea for easier editing
    public LanguageTextPair(Language language,string message) {
        this.language = language;
        this.message = message;
    }

    internal bool IsSameAs(string compareMessage) {
        return string.Equals(message,compareMessage);
    }
}

[Serializable]
public class LegacyLanguageFontPair {
    public Language language;
    public Font font;
    public LegacyLanguageFontPair(Language language,Font font) {
        this.language = language;
        this.font = font;
    }
}

public class LanguageUpdate:MonoBehaviour {

    // --- Manager Reference ---
    private LanguageManager _managerInstance;
    private LanguageManager ManagerInstance {
        get {
            if (this == null)
                return null;
            if (_managerInstance == null) {
                _managerInstance = FindAnyObjectByType<LanguageManager>();
            }
            return _managerInstance;
        }
    }

    // --- Component References ---

    // Changed from 'TextMeshProUGUI' to 'TMP_Text' to support BOTH 3D and UI TMP
    private TMP_Text _tmpComponent;
    private TMP_Text TmpComponent {
        get {
            if (_tmpComponent == null)
                _tmpComponent = GetComponent<TMP_Text>();
            return _tmpComponent;
        }
    }

    private Text _legacyTextComponent;
    private Text LegacyTextComponent {
        get {
            if (_legacyTextComponent == null)
                _legacyTextComponent = GetComponent<Text>();
            return _legacyTextComponent;
        }
    }

    private bool _isTMP; // True = TextMeshPro (UI or 3D), False = Legacy Text

    // --- Data Lists ---
    private List<LanguageFontPair> _localFontAssetList = new List<LanguageFontPair>();
    [SerializeField] private List<LegacyLanguageFontPair> _localLegacyFontList = new List<LegacyLanguageFontPair>();
    [SerializeField] private List<LanguageTextPair> _localTextList = new List<LanguageTextPair>();

    private void Awake() {
        // Check for TMP_Text (covers both TextMeshPro and TextMeshProUGUI)
        if (GetComponent<TMP_Text>() != null) {
            _isTMP = true;
            _localFontAssetList.Clear();
            if (TmpComponent.font != null) {
                _localFontAssetList.Add(new LanguageFontPair(Language.English,TmpComponent.font));
            }
        }
        // Check for Legacy Text
        else if (GetComponent<Text>() != null) {
            _isTMP = false;
            if (LegacyTextComponent.font != null && !_localLegacyFontList.Exists(x => x.language == Language.English)) {
                _localLegacyFontList.Add(new LegacyLanguageFontPair(Language.English,LegacyTextComponent.font));
            }
        }
    }

    private void OnEnable() {
        // Subscribe to static event
        LanguageManager.OnLanguageChangedEvent += UpdateFontAsset;
        LanguageManager.OnLanguageChangedEvent += UpdateFontText;

        if (ManagerInstance != null) {
            UpdateFontAsset(ManagerInstance.CurrentLanguage);
            UpdateFontText(ManagerInstance.CurrentLanguage);
        }
    }

    private void OnDestroy() {
        LanguageManager.OnLanguageChangedEvent -= UpdateFontAsset;
        LanguageManager.OnLanguageChangedEvent -= UpdateFontText;
    }

    #region Update_FontAsset
    private TMP_FontAsset GetFontForCurrentLanguage(Language language) {
        return _localFontAssetList.Find(pair => pair.language == language)?.fontAsset;
    }

    private Font GetLegacyFontForCurrentLanguage(Language language) {
        return _localLegacyFontList.Find(pair => pair.language == language)?.font;
    }

    private void UpdateFontAsset(Language language) {
        if (this == null)
            return;

        if (_isTMP) {
            // --- Unified TMP Logic (Works for 3D & UI) ---
            TMP_FontAsset fontAsset = GetFontForCurrentLanguage(language);
            if (fontAsset == null) {
                fontAsset = InitializeMarathiFont(language);
            }
            if (fontAsset != null) {
                TmpComponent.font = fontAsset;
                TmpComponent.fontStyle = FontStyles.Normal;
            }
        } else {
            // --- Legacy Text Logic ---
            Font font = GetLegacyFontForCurrentLanguage(language);
            if (font != null) {
                LegacyTextComponent.font = font;
                LegacyTextComponent.fontStyle = FontStyle.Normal;
            }
        }
    }

    private TMP_FontAsset InitializeMarathiFont(Language language) {
        if (ManagerInstance == null)
            return null;

        TMP_FontAsset fontAsset = ManagerInstance.GetFontForCurrentLanguage(language);
        _localFontAssetList.Add(new LanguageFontPair(language,fontAsset));
        return fontAsset;
    }
    #endregion

    #region Update_FontText   
    private void UpdateFontText(Language language) {
        if (this == null)
            return;

        if (IsLocalTextAvailable(language)) {
            LanguageTextPair foundPair = GetTextPair(language);
            if (foundPair != null) {
                string finalMessage = foundPair.message;

                if (language == Language.Marathi && ManagerInstance != null) {
                    finalMessage = ManagerInstance.GetMarathiTextParser().GetMarathiText(foundPair.message);
                }

                if (_isTMP) {
                    TmpComponent.text = finalMessage;
                } else {
                    LegacyTextComponent.text = finalMessage;
                }
            }
        }
    }

    private bool IsLocalTextAvailable(Language language) {
        return _localTextList.Exists(x => x.language.Equals(language));
    }

    private LanguageTextPair GetTextPair(Language language) {
        return _localTextList.Find(x => x.language.Equals(language));
    }

    internal void ManualUpdate(string message) {
        if (this == null)
            return;

        if (ManagerInstance != null) {
            UpdateFontAsset(ManagerInstance.CurrentLanguage);
        }

        if (_isTMP) {
            TmpComponent.text = message;
        } else {
            LegacyTextComponent.text = message;
        }
    }

    //[ContextMenu("EnterEnglishData")]
    //private void EnterEnglishData() {
    //    _localTextList[0].message = TmpComponent.text;
    //}
    //internal string GetName() {
    //    return _localTextList[0].message;
    //}
    //public void AssignMrName(string marathiName) {
    //    _localTextList[1].message = marathiName;
    //}
    #endregion
}