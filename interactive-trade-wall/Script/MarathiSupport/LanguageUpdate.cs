using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    // =========================
    // TEXT REFERENCES
    // =========================

    private TextMeshProUGUI _languageTextTMP;
    private Text _languageTextLegacy;

    private bool HasTMP => _languageTextTMP != null;
    private bool HasLegacy => _languageTextLegacy != null;

    private void CacheTextComponent() {
        if (_languageTextTMP == null && _languageTextLegacy == null) {
            _languageTextTMP = GetComponent<TextMeshProUGUI>();
            _languageTextLegacy = GetComponent<Text>();
        }
    }

    // =========================
    // DATA
    // =========================

    private readonly List<LanguageFontPair> _localFontAssetList = new();
    [SerializeField] private List<LanguageTextPair> _localTextList = new();

    private MarathiTextParser _marathiTextParser;

    // =========================
    // UNITY
    // =========================

    private void Awake() {
        CacheTextComponent();

        _localFontAssetList.Clear();

        if (HasTMP) {
            _localFontAssetList.Add(
                new LanguageFontPair(Language.English,_languageTextTMP.font)
            );
        }
    }

    private void OnEnable() {
        LanguageManager.OnLanguageChangedEvent += UpdateFontAsset;
        LanguageManager.OnLanguageChangedEvent += UpdateFontText;
    }

    private void OnDestroy() {
        if (_languageManager == null)
            return;

        LanguageManager.OnLanguageChangedEvent -= UpdateFontAsset;
        LanguageManager.OnLanguageChangedEvent -= UpdateFontText;
    }

    // =========================
    // FONT UPDATE
    // =========================

    private TMP_FontAsset GetFontForCurrentLanguage(Language language) {
        return _localFontAssetList.Find(pair => pair.language == language)?.fontAsset;
    }

    private void UpdateFontAsset(Language language) {
        if (!HasTMP)
            return; // Legacy UI can't use TMP fonts

        TMP_FontAsset fontAsset = GetFontForCurrentLanguage(language);

        if (fontAsset == null) {
            fontAsset = LanguageManager.GetFontForCurrentLanguage(language);

            if (fontAsset != null) {
                _localFontAssetList.Add(
                    new LanguageFontPair(language,fontAsset)
                );
            }
        }

        if (fontAsset != null)
            _languageTextTMP.font = fontAsset;
    }

    // =========================
    // TEXT UPDATE
    // =========================

    private void UpdateFontText(Language language) {
        if (!IsLocalTextAvailable(language))
            return;

        LanguageTextPair foundPair = GetTextPair(language);

        if (foundPair == null)
            return;

        string finalText = foundPair.message;

        if (language == Language.Marathi) {
            finalText = LanguageManager
                .GetMarathiTextParser()
                .GetMarathiText(foundPair.message);
        }

        ApplyText(finalText);
    }

    private void ApplyText(string value) {
        if (HasTMP)
            _languageTextTMP.text = value;
        else if (HasLegacy)
            _languageTextLegacy.text = value;
    }

    private bool IsLocalTextAvailable(Language language) {
        return _localTextList.Exists(x => x.language == language);
    }

    private LanguageTextPair GetTextPair(Language language) {
        return _localTextList.Find(x => x.language == language);
    }

    internal void ManualUpdate(string message) {
        UpdateFontAsset(LanguageManager.CurrentLanguage);
        ApplyText(message);
    }
}
