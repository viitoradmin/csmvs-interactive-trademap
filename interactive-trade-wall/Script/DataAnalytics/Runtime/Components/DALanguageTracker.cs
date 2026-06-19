// ============================================================
// DataAnalytics v1.0.0
// DALanguageTracker.cs
// Tracks language selection count and reading duration for the
// currently-active application language.
// ============================================================

using UnityEngine;
using DataAnalytics.Runtime.Managers;
using DataAnalytics.Runtime.Utilities;

namespace DataAnalytics.Runtime.Components
{
    /// <summary>
    /// Component that tracks how many times each language is selected and how long
    /// users spend reading in that language.
    ///
    /// <para><b>Setup:</b> Attach this component to the GameObject that owns your
    /// language-switching logic (e.g. the object with <c>LanguageButtonHandler</c>),
    /// then call <see cref="SetActiveLanguage(string)"/> from that script whenever the
    /// active language changes. No singleton — one instance lives wherever you attach it.</para>
    ///
    /// <para>The active UI language is global state (one language at a time), so this
    /// tracker is language-agnostic: pass the display name of the now-active language
    /// and it handles selection counting and reading-duration timing internally.</para>
    /// </summary>
    public class DALanguageTracker : MonoBehaviour
    {
        // ────────────────────────────────────────────────────────────────────────
        // Runtime state
        // ────────────────────────────────────────────────────────────────────────

        private string _activeLanguage = string.Empty;
        private float  _segmentStartTime;
        private bool   _hasActive;

        /// <summary>The language currently being tracked, or empty if none yet.</summary>
        public string ActiveLanguage => _activeLanguage;

        // ────────────────────────────────────────────────────────────────────────
        // Unity lifecycle
        // ────────────────────────────────────────────────────────────────────────

        // Commit the in-progress reading segment whenever the app may be backgrounded,
        // closed, or this object is disabled, so duration is never lost on an unclean
        // shutdown. The timer is restarted (rather than stopped) on pause/focus so
        // reading resumes cleanly afterwards.
        private void OnApplicationPause(bool paused) { if (paused) CommitCurrentSegment(restart: true); }
        private void OnApplicationFocus(bool focus)  { if (!focus) CommitCurrentSegment(restart: true); }
        private void OnApplicationQuit()             { CommitCurrentSegment(restart: false); }
        private void OnDisable()                     { CommitCurrentSegment(restart: false); }

        // ────────────────────────────────────────────────────────────────────────
        // Public API
        // ────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Sets the active language. Records a selection for the new language,
        /// commits the reading duration accumulated for the previously-active
        /// language, and starts timing the new one.
        ///
        /// <para>Repeated calls with the same language are ignored, so it is safe to
        /// call this from code paths that may run more than once per actual change
        /// (e.g. UI-refresh or data-fetched callbacks).</para>
        /// </summary>
        /// <param name="languageName">
        /// Display name of the now-active language (e.g. "English", "Marathi").
        /// Used verbatim in analytics reports.
        /// </param>
        public void SetActiveLanguage(string languageName)
        {
            if (string.IsNullOrWhiteSpace(languageName)) return;

            // Ignore redundant notifications for the language already being tracked.
            if (_hasActive && languageName == _activeLanguage) return;

            // Close out the previous language's reading segment.
            CommitCurrentSegment(restart: false);

            // Count the new selection and begin timing it.
            DAAnalyticsManager.Instance?.RecordLanguageSelect(languageName);

            _activeLanguage   = languageName;
            _segmentStartTime = Time.realtimeSinceStartup;
            _hasActive        = true;

            DALogger.Log($"Language tracking switched to: {languageName}");
        }

        // ────────────────────────────────────────────────────────────────────────
        // Internal
        // ────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Commits the elapsed reading time for the active language to the manager.
        /// </summary>
        /// <param name="restart">
        /// When <c>true</c>, the segment timer is reset so tracking continues for the
        /// same language; when <c>false</c>, tracking stops until the next call to
        /// <see cref="SetActiveLanguage(string)"/>.
        /// </param>
        private void CommitCurrentSegment(bool restart)
        {
            if (!_hasActive) return;

            float elapsed = Time.realtimeSinceStartup - _segmentStartTime;
            if (elapsed > 0f)
                DAAnalyticsManager.Instance?.RecordLanguageDuration(_activeLanguage, elapsed);

            if (restart)
                _segmentStartTime = Time.realtimeSinceStartup;
            else
                _hasActive = false;
        }
    }
}
