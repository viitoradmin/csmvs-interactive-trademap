// ============================================================
// DataAnalytics v1.0.0
// DAAnalyticsManager.cs
// Main singleton — holds analytics in memory, loads/saves JSON.
// ============================================================

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataAnalytics.Runtime.Data;
using DataAnalytics.Runtime.Storage;
using DataAnalytics.Runtime.Utilities;

namespace DataAnalytics.Runtime.Managers
{
    /// <summary>
    /// Central singleton that owns all analytics data for the current week.
    /// Survives scene loads via <see cref="DontDestroyOnLoad"/>.
    ///
    /// <para><b>Usage:</b> Access via <see cref="Instance"/>. Components call the
    /// Record* methods — no additional code required in the host project.</para>
    /// </summary>
    public class DAAnalyticsManager : MonoBehaviour
    {
        // ────────────────────────────────────────────────────────────────────────
        // Singleton
        // ────────────────────────────────────────────────────────────────────────

        private static DAAnalyticsManager _instance;

        /// <summary>
        /// Global singleton instance. Auto-creates the manager if it does not
        /// already exist in the scene (via <see cref="GetOrCreate"/>).
        /// </summary>
        public static DAAnalyticsManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = GetOrCreate();
                return _instance;
            }
        }

        private static DAAnalyticsManager GetOrCreate()
        {
            DAAnalyticsManager existing = FindObjectOfType<DAAnalyticsManager>();
            if (existing != null) return existing;

            GameObject go = new GameObject("[DA] AnalyticsManager");
            return go.AddComponent<DAAnalyticsManager>();
        }

        // ────────────────────────────────────────────────────────────────────────
        // Inspector
        // ────────────────────────────────────────────────────────────────────────

        [Header("Settings Override (leave empty to use Resources/DASettings.asset)")]
        [Space(4)]

        [Tooltip("Optional direct reference to a DASettings asset. If left empty the manager loads from Resources automatically.")]
        [SerializeField] private DASettings _settingsOverride;

        // ────────────────────────────────────────────────────────────────────────
        // Runtime state
        // ────────────────────────────────────────────────────────────────────────

        private DAAnalyticsData _data;
        private Coroutine       _autosaveCoroutine;
        private bool            _initialized;

        /// <summary>Read-only access to the current analytics snapshot.</summary>
        public DAAnalyticsData Data => _data;

        // ────────────────────────────────────────────────────────────────────────
        // Unity lifecycle
        // ────────────────────────────────────────────────────────────────────────

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            Initialize();
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused) SaveNow();
        }

        private void OnApplicationFocus(bool focus)
        {
            if (!focus) SaveNow();
        }

        private void OnApplicationQuit()
        {
            SaveNow();
        }

        // ────────────────────────────────────────────────────────────────────────
        // Initialization
        // ────────────────────────────────────────────────────────────────────────

        private void Initialize()
        {
            if (_initialized) return;

            // Apply settings override if provided, otherwise load from Resources
            if (_settingsOverride != null)
                DASettingsLoader.SetOverride(_settingsOverride);

            // Ensure all directories exist
            DAStorageManager.EnsureDirectories();

            // Load existing data or create fresh week
            LoadOrCreateAnalytics();

            // Start autosave coroutine
            StartAutosave();

            _initialized = true;
        }

        private void LoadOrCreateAnalytics()
        {
            DAAnalyticsData loaded = DAStorageManager.LoadCurrentAnalytics();
            string currentWeek    = DATimeUtility.GetCurrentWeekStart();

            if (loaded != null && loaded.weekStartDate == currentWeek)
            {
                _data = loaded;
            }
            else
            {
                if (loaded != null)
                {
                    // A new week started while the app was closed. Archive the previous
                    // week's file before overwriting it so no analytics are ever lost
                    // (e.g. activity recorded after the Saturday report but before Monday).
                    DAStorageManager.ArchiveCurrentFile(loaded.weekStartDate);
                }

                _data = DAAnalyticsData.CreateNew(currentWeek);
                SaveNow();
            }
        }

        // ────────────────────────────────────────────────────────────────────────
        // Autosave
        // ────────────────────────────────────────────────────────────────────────

        private void StartAutosave()
        {
            if (_autosaveCoroutine != null)
                StopCoroutine(_autosaveCoroutine);

            _autosaveCoroutine = StartCoroutine(AutosaveRoutine());
        }

        private IEnumerator AutosaveRoutine()
        {
            DASettings settings = DASettingsLoader.Settings;
            float interval = settings != null ? settings.SaveIntervalSeconds : 30f;

            while (true)
            {
                yield return new WaitForSecondsRealtime(interval);
                SaveNow();
            }
        }

        /// <summary>
        /// Forces an immediate save of the current analytics data to disk.
        /// Called automatically by autosave, application pause/focus/quit.
        /// </summary>
        public void SaveNow()
        {
            if (_data == null) return;
            DAStorageManager.SaveCurrentAnalytics(_data);
        }

        // ────────────────────────────────────────────────────────────────────────
        // Public Record API — called by components
        // ────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Increments the click counter for the given product by 1.
        /// </summary>
        /// <param name="productName">Product name as configured on the component.</param>
        public void RecordProductClick(string productName)
        {
            if (string.IsNullOrWhiteSpace(productName)) return;
            DAProductAnalytics entry = GetOrCreateProduct(productName);
            entry.clickCount++;
        }

        /// <summary>
        /// Increments the selection count for the given language by 1.
        /// </summary>
        /// <param name="languageName">Language name as configured on the component.</param>
        public void RecordLanguageSelect(string languageName)
        {
            if (string.IsNullOrWhiteSpace(languageName)) return;
            DALanguageAnalytics entry = GetOrCreateLanguage(languageName);
            entry.selectionCount++;
        }

        /// <summary>
        /// Adds the given duration (in seconds) to the reading time for a language.
        /// </summary>
        /// <param name="languageName">Language name as configured on the component.</param>
        /// <param name="seconds">Duration in seconds to add.</param>
        public void RecordLanguageDuration(string languageName, float seconds)
        {
            if (string.IsNullOrWhiteSpace(languageName) || seconds <= 0f) return;
            DALanguageAnalytics entry = GetOrCreateLanguage(languageName);
            entry.totalSeconds += seconds;
        }

        /// <summary>
        /// Increments the visit count for the given screen by 1.
        /// </summary>
        /// <param name="screenName">Screen name as configured on the component.</param>
        public void RecordScreenVisit(string screenName)
        {
            if (string.IsNullOrWhiteSpace(screenName)) return;
            DAScreenAnalytics entry = GetOrCreateScreen(screenName);
            entry.visitCount++;
        }

        /// <summary>
        /// Adds the given duration (in seconds) to the time spent on a screen.
        /// </summary>
        /// <param name="screenName">Screen name as configured on the component.</param>
        /// <param name="seconds">Duration in seconds to add.</param>
        public void RecordScreenDuration(string screenName, float seconds)
        {
            if (string.IsNullOrWhiteSpace(screenName) || seconds <= 0f) return;
            DAScreenAnalytics entry = GetOrCreateScreen(screenName);
            entry.totalSeconds += seconds;
        }

        /// <summary>
        /// Adds the given idle duration (in seconds) to the weekly idle total.
        /// </summary>
        /// <param name="seconds">Idle duration in seconds to add.</param>
        public void RecordIdleTime(float seconds)
        {
            if (seconds <= 0f) return;
            _data.idle.totalIdleSeconds += seconds;
        }

        /// <summary>
        /// Resets all in-memory analytics to a fresh state for a new week.
        /// Should be called by <see cref="DAAnalyticsScheduler"/> after report generation.
        /// </summary>
        public void ResetForNewWeek()
        {
            string newWeekStart = DATimeUtility.GetCurrentWeekStart();
            _data = DAAnalyticsData.CreateNew(newWeekStart);
            SaveNow();
        }

        // ────────────────────────────────────────────────────────────────────────
        // Internal get-or-create helpers
        // ────────────────────────────────────────────────────────────────────────

        private DAProductAnalytics GetOrCreateProduct(string name)
        {
            foreach (DAProductAnalytics p in _data.products)
                if (p.productName == name) return p;

            DAProductAnalytics entry = new DAProductAnalytics { productName = name };
            _data.products.Add(entry);
            return entry;
        }

        private DALanguageAnalytics GetOrCreateLanguage(string name)
        {
            foreach (DALanguageAnalytics l in _data.languages)
                if (l.languageName == name) return l;

            DALanguageAnalytics entry = new DALanguageAnalytics { languageName = name };
            _data.languages.Add(entry);
            return entry;
        }

        private DAScreenAnalytics GetOrCreateScreen(string name)
        {
            foreach (DAScreenAnalytics s in _data.screens)
                if (s.screenName == name) return s;

            DAScreenAnalytics entry = new DAScreenAnalytics { screenName = name };
            _data.screens.Add(entry);
            return entry;
        }
    }
}
