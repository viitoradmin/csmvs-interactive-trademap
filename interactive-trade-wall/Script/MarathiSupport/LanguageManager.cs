using System;
using System.IO;
using TMPro;
using UnityEngine;

public enum Language {
    English,
    Marathi,
}
[Serializable]
public class LanguageFontPair {
    public Language language;
    public TMP_FontAsset fontAsset;

    public LanguageFontPair(Language language,TMP_FontAsset fontAsset) {
        this.language = language;
        this.fontAsset = fontAsset;
    }
}
public class LanguageManager: MonoBehaviour {
    //[SerializeField] private APIHandler apiHandler;
    [SerializeField] private InteractiveTradeWallDataSO data;
    [SerializeField] private TextAsset InteractiveTradeWallDataSO_En;
    [SerializeField] private TextAsset InteractiveTradeWallDataSO_Mr;

    private Language _currentLanguage = Language.English;
    public Language CurrentLanguage {
        get { return _currentLanguage; }
        set {
            _currentLanguage = value;
            switch (_currentLanguage) {
                case Language.English:
                    //apiHandler.GetDataForEnglish();
                    EnglishSelected();
                break;
                case Language.Marathi:
                    //apiHandler.GetDataForMarathi();
                    MarathiSelected();
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
    //private void OnEnable() {
    //    APIHandler.OnDataFetchedEvent += OnDataFetched;
    //}
    //private void OnDisable() {
    //    APIHandler.OnDataFetchedEvent -= OnDataFetched;
    //}
    //private void OnDataFetched(Root root) {        
    //    OnLanguageChangedEvent?.Invoke(_currentLanguage);
    //}   
    //[ContextMenu("SetEnglishLanguage")]
    //public void SetEnglishLanguage() {
    //    CurrentLanguage = Language.English;
    //}
    //[ContextMenu("SetMarathiLanguage")]
    //public void SetMarathiLanguage() {
    //    CurrentLanguage = Language.Marathi;
    //}
    internal TMP_FontAsset GetFontForCurrentLanguage(Language language) {
        return Array.Find(fonts, pair => pair.language == language).fontAsset;
    }
    internal MarathiTextParser GetMarathiTextParser() => _marathiTextParser;

    [ContextMenu("StoreData")]
    public void StoreData() {
        string jsonData =  JsonUtility.ToJson(data.root);
        File.WriteAllText(Application.persistentDataPath + "/StoredData.json", jsonData);
        Debug.Log("Data Stored at: " + Application.persistentDataPath + "/StoredData.json");
    }
    private void EnglishSelected() {
        string jsonData = InteractiveTradeWallDataSO_En.text;
        data.root = JsonUtility.FromJson<InteractiveTradeWallDataSO.Root>(jsonData);
    }
    private void MarathiSelected() {
        string jsonData = InteractiveTradeWallDataSO_Mr.text;
        data.root = JsonUtility.FromJson<InteractiveTradeWallDataSO.Root>(jsonData);
    }
}//LanguageManager class end.
