using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static InteractiveTradeWallDataSO;

public class LanguageButtonHandler:MonoBehaviour {
    [SerializeField] private Button englishButton;
    [SerializeField] private Button marathiButton;
    private List<Button> buttons = new List<Button>();
    //[SerializeField] private AppController appController;
    private void OnEnable() {
        APIHandler.OnDataFetchedEvent += OnDataFetched;
    }
    private void OnDestroy() {
        APIHandler.OnDataFetchedEvent -= OnDataFetched;
    }
    private void OnDataFetched(Root root) {
        RefreshSelectedButtonUI();
    }
    private void Start() {
        buttons.Clear();
        buttons.Add(englishButton);
        buttons.Add(marathiButton);

        englishButton.onClick.RemoveAllListeners();
        englishButton.onClick.AddListener(() => {
            OnLanguageToggleTo(Language.Marathi);
        });

        marathiButton.onClick.RemoveAllListeners();
        marathiButton.onClick.AddListener(() => {
            OnLanguageToggleTo(Language.English);
        });
    }
    private void OnLanguageToggleTo(Language selectedLanguage) {
        LanguageManager.Instance.CurrentLanguage = selectedLanguage;
        RefreshSelectedButtonUI();
    }
    private void RefreshSelectedButtonUI() {
        DisableAllButtons();
        buttons[(int)LanguageManager.Instance.CurrentLanguage % buttons.Count].gameObject.SetActive(true);
    }
    private void DisableAllButtons() {
        buttons.ForEach(button => button.gameObject.SetActive(false));
    }
}//LanguageButtonHandler class end.
