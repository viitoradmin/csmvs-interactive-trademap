using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DataAnalytics.Runtime.Components;
using static InteractiveTradeWallDataSO;

public class LanguageButtonHandler:MonoBehaviour {
    [SerializeField] private Button englishButton;
    [SerializeField] private Button marathiButton;
    [Tooltip("Analytics tracker for language usage. Attach DALanguageTracker to this GameObject; auto-resolved if left empty.")]
    [SerializeField] private DALanguageTracker daLanguageTracker;
    private List<Button> buttons = new List<Button>();
    //[SerializeField] private AppController appController;
    private void OnEnable() {
        APIHandler.OnDataFetchedEvent += OnDataFetched;
        BookController.onEffectChangingEvent += OnEffectChange;
    }
    private void OnDestroy() {
        APIHandler.OnDataFetchedEvent -= OnDataFetched;
        BookController.onEffectChangingEvent -= OnEffectChange;
    }
    private void OnEffectChange(bool isChanegd) {
        buttons.ForEach(button => button.interactable = !isChanegd);
    }
    private void OnDataFetched(Root root) {
        if (!root.languageSwitchButton.isEnable) { 
            DisableAllButtons();
            return;
        }
        RefreshSelectedButtonUI();
    }
    private void Awake() {
        if (daLanguageTracker == null)
            daLanguageTracker = GetComponent<DALanguageTracker>();

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

        // Track the now-active language for analytics (dedup handles repeated refreshes).
        if (daLanguageTracker != null)
            daLanguageTracker.SetActiveLanguage(LanguageManager.Instance.CurrentLanguage.ToString());
    }
    private void DisableAllButtons() {
        buttons.ForEach(button => button.gameObject.SetActive(false));
    }
}//LanguageButtonHandler class end.
