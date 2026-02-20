using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI; // Required for UnityEngine.UI.Text

[Serializable]
public class LanguageTextPair {
    public Language language;
    public string message;
    public LanguageTextPair(Language language,string message) {
        this.language = language;
        this.message = message;
    }

    internal bool IsSameAs(string compareMessage) {
        return string.Equals(message,compareMessage);
    }
}

// New class to handle standard Fonts for Legacy Text
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
    private LanguageManager _languageManager;
    private LanguageManager LanguageManager {
        get {
            if (_languageManager == null) {
                _languageManager = FindAnyObjectByType<LanguageManager>();
            }
            return _languageManager;
        }
    }

    // TMP Reference
    private TMP_Text _languageTextTMP;
    private TMP_Text LanguageTextTMP {
        get {
            if (_languageTextTMP == null) {
                TryGetComponent<TMP_Text>(out _languageTextTMP);
            }
            return _languageTextTMP;
        }
    }

    // Legacy Text Reference
    private Text _languageTextLegacy;
    private Text LanguageTextLegacy {
        get {
            if (_languageTextLegacy == null) {
                TryGetComponent<Text>(out _languageTextLegacy);
            }
            return _languageTextLegacy;
        }
    }

    // Flag to determine which component is active
    private bool _isTMP;

    // Font Lists
    private List<LanguageFontPair> _localFontAssetList = new List<LanguageFontPair>(); // For TMP
    [SerializeField] private List<LegacyLanguageFontPair> _localLegacyFontList = new List<LegacyLanguageFontPair>(); // For Legacy Text

    [SerializeField] private List<LanguageTextPair> _localTextList = new List<LanguageTextPair>();
    
    public List<LanguageTextPair> LocalTextList { get => _localTextList; set => _localTextList = value; }
    private void Awake() {
        // Detect which component is present
        if (GetComponent<TMP_Text>() != null) {
            _isTMP = true;
            _localFontAssetList.Clear();
            if (LanguageTextTMP.font != null) {
                _localFontAssetList.Add(new LanguageFontPair(Language.English,LanguageTextTMP.font));
            }
        } else if (GetComponent<Text>() != null) {
            _isTMP = false;
            // Optional: Auto-add current font to legacy list if not present
            if (LanguageTextLegacy.font != null && !_localLegacyFontList.Exists(x => x.language == Language.English)) {
                _localLegacyFontList.Add(new LegacyLanguageFontPair(Language.English,LanguageTextLegacy.font));
            }
        }
    }
    private void OnEnable() {
        LanguageManager.OnLanguageChangedEvent += UpdateFontAsset;
        LanguageManager.OnLanguageChangedEvent += UpdateFontText;


        UpdateFontAsset(LanguageManager.CurrentLanguage);
        UpdateFontText(LanguageManager.CurrentLanguage);
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
        if (_isTMP) {
            // TMP Logic
            TMP_FontAsset fontAsset = GetFontForCurrentLanguage(language);
            if (fontAsset == null) {
                fontAsset = InitializeMarathiFont(language);
            }
            if (fontAsset != null) {
                LanguageTextTMP.font = fontAsset;
                LanguageTextTMP.fontStyle = FontStyles.Normal;
            }
        } else {
            // Legacy Text Logic
            // Note: Since LanguageManager likely returns TMP_FontAsset, we rely on _localLegacyFontList 
            // to be populated in the Inspector for Legacy Fonts.
            Font font = GetLegacyFontForCurrentLanguage(language);
            if (font == null) {
                font = InitializeMarathiFontLagacy(language);
            }
            if (font != null) {
                LanguageTextLegacy.font = font;
                LanguageTextLegacy.fontStyle = FontStyle.Normal;
            }
        }
    }
    private TMP_FontAsset InitializeMarathiFont(Language language) {
        // This method assumes LanguageManager returns a TMP_FontAsset
        TMP_FontAsset fontAsset = LanguageManager.GetFontForCurrentLanguage(language);
        _localFontAssetList.Add(new LanguageFontPair(language,fontAsset));
        return fontAsset;
    }
    private Font InitializeMarathiFontLagacy(Language language) {
        // This method assumes LanguageManager returns a TMP_FontAsset
        Font fontAsset = LanguageManager.GetFontForCurrentLanguageLagacy(language);
        _localLegacyFontList.Add(new LegacyLanguageFontPair(language,fontAsset));
        return fontAsset;
    }
    #endregion

    #region Update_FontText   
    private void UpdateFontText(Language language) {
        if (IsLocalTextAvailable(language)) {
            LanguageTextPair foundPair = GetTextPair(language);
            if (foundPair != null) {
                string finalMessage = foundPair.message;

                // Handle Marathi Parsing
                if (language == Language.Marathi) {
                    finalMessage = LanguageManager.GetMarathiTextParser().GetMarathiText(foundPair.message);
                }

                // Apply to active component
                if (_isTMP) {
                    LanguageTextTMP.text = finalMessage;
                } else {
                    LanguageTextLegacy.text = finalMessage;
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
        UpdateFontAsset(LanguageManager.CurrentLanguage);
        if (_isTMP) {
            LanguageTextTMP.text = message;
        } else {
            LanguageTextLegacy.text = message;
        }
    }
    #endregion

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
}//LanguageUpdate class end.