using System;
using TMPro;
using UnityEngine;
using static InteractiveTradeWallDataSO;
using System.Collections.Generic;

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
                break;
                case Language.Marathi:
                    apiHandler.GetDataForMarathi();
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

    //[SerializeField] private List<LanguageUpdate> lan;
    //[ContextMenu("Print")]
    //public void PrintName() {
    //    List<string> data = lan.ConvertAll(l => l.GetName());
    //    Debug.Log(string.Join(",\n", data));
    //}
    //[TextArea] public string marathiNames;
    //[ContextMenu("AssignMarathiName")]
    //public void AssignMarathiName() {
    //    // Split the marathiNames by ,
    //    string[] names = marathiNames.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
    //    // and assign to lan list
    //    for (int i = 0; i < names.Length; i++) {
    //        if (i < lan.Count) {
    //            lan[i].AssignMrName(names[i].Trim());
    //        }
    //    }
    //    //print assigned count
    //    Debug.Log($"Assigned Marathi Names to {Math.Min(names.Length, lan.Count)} items.");
    //}
}//LanguageManager class end.
