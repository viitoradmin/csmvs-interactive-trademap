using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

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
[RequireComponent(typeof(TextMeshProUGUI))]
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
    private TextMeshProUGUI _languageText;
    private TextMeshProUGUI LanguageText {
        get {
            if (_languageText == null) {
                _languageText = GetComponent<TextMeshProUGUI>();
            }
            return _languageText;
        }
    }
    private List<LanguageFontPair> _localFontAssetList = new List<LanguageFontPair>();
    [SerializeField] private List<LanguageTextPair> _localTextList = new List<LanguageTextPair>();
    private MarathiTextParser _marathiTextParser;
   
    private void Awake() {
        _localFontAssetList.Clear();
        _localFontAssetList.Add(new LanguageFontPair(Language.English,LanguageText.font));
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
    private void UpdateFontAsset(Language language) {        
        TMP_FontAsset fontAsset = GetFontForCurrentLanguage(language);
        if (fontAsset == null) {
            fontAsset = InitializeMarathiFont(language);
        }
        LanguageText.font = fontAsset;
        LanguageText.fontStyle = FontStyles.Normal;
    }

    private TMP_FontAsset InitializeMarathiFont(Language language) {
        TMP_FontAsset fontAsset = LanguageManager.GetFontForCurrentLanguage(language);
        _localFontAssetList.Add(new LanguageFontPair(language,fontAsset));
        return fontAsset;
    }
    #endregion

    #region Update_FontText   
    private void UpdateFontText(Language language) {
        if (IsLocalTextAvailable(language)) {
            LanguageTextPair foundPair = GetTextPair(language);
            if (foundPair != null) {
                switch (language) {
                    case Language.English:
                    LanguageText.text = foundPair.message;
                    break;
                    case Language.Marathi:
                    string vakraText = LanguageManager.GetMarathiTextParser().GetMarathiText(foundPair.message);
                    LanguageText.text = vakraText;
                    break;
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
        LanguageText.text = message;
    }
    #endregion
}//LanguageUpdate class end.