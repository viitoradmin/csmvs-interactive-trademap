using System;
using TMPro;
using UnityEngine;
using static InteractiveTradeWallDataSO;

public enum Language {
    English,
    Marathi,
}
[Serializable]
public class LanguageFontPair {
    public Language language;
    public TMP_FontAsset fontAsset;
    public Font lagacyFontAsset;
    public LanguageFontPair() { }
    public LanguageFontPair(Language language,TMP_FontAsset fontAsset) {
        this.language = language;
        this.fontAsset = fontAsset;
    }
    public LanguageFontPair(Language language,Font lagacyFontAsset) {
        this.language = language;
        this.lagacyFontAsset = lagacyFontAsset;
    }
}
public class LanguageManager: MonoBehaviour {
    [SerializeField] private APIHandler apiHandler;

    private Language _currentLanguage = Language.English;
    public Language CurrentLanguage {
        get { return _currentLanguage; }
        set {
            _currentLanguage = value;
            switch (_currentLanguage) {
                case Language.English:
                    apiHandler.GetDataForEnglish();
                    //EnglishSelected();
                break;
                case Language.Marathi:
                    apiHandler.GetDataForMarathi();
                    //MarathiSelected();
                break;
            }
            OnLanguageChangedEvent?.Invoke(_currentLanguage);
        }
    }
    public static Action<Language> OnLanguageChangedEvent = delegate { };

    [SerializeField] private LanguageFontPair[] fonts;

    [SerializeField] private MarathiTextParser _marathiTextParser;
    public static LanguageManager Instance;
    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }
    private void OnEnable() {
        APIHandler.OnDataFetchedEvent += OnDataFetched;
    }
    private void OnDisable() {
        APIHandler.OnDataFetchedEvent -= OnDataFetched;
    }
    private void OnDataFetched(Root root) {
        OnLanguageChangedEvent?.Invoke(_currentLanguage);
    }
    internal TMP_FontAsset GetFontForCurrentLanguage(Language language) {
        return Array.Find(fonts, pair => pair.language == language).fontAsset;
    }
    internal Font GetFontForCurrentLanguageLagacy(Language language) {
        return Array.Find(fonts, pair => pair.language == language).lagacyFontAsset;
    }
    internal MarathiTextParser GetMarathiTextParser() => _marathiTextParser;
}//LanguageManager class end.
