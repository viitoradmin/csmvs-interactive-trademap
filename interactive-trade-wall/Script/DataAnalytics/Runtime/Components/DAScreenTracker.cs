// ============================================================
// DataAnalytics v1.0.0
// DAScreenTracker.cs
// Tracks screen visit count and total time spent per panel/canvas.
// ============================================================

using UnityEngine;
using DataAnalytics.Runtime.Managers;
using DataAnalytics.Runtime.Utilities;

namespace DataAnalytics.Runtime.Components
{
    /// <summary>
    /// Tracks how many times a screen (panel, canvas, or any GameObject) is
    /// visited and the cumulative time users spend on it.
    ///
    /// <para><b>Setup:</b> Attach this component to the root GameObject of any
    /// screen that is shown/hidden via <see cref="GameObject.SetActive"/>.
    /// Set <see cref="_screenName"/> in the Inspector.
    /// No additional code required.</para>
    ///
    /// <para>Visit count increments in <see cref="OnEnable"/>.
    /// Duration is measured from <see cref="OnEnable"/> to <see cref="OnDisable"/>.</para>
    /// </summary>
    public class DAScreenTracker : MonoBehaviour
    {
        // ────────────────────────────────────────────────────────────────────────
        // Inspector
        // ────────────────────────────────────────────────────────────────────────

        [Header("Screen Identity")]
        [Space(4)]

        [Tooltip("Unique screen name displayed in analytics reports. " +
                 "Examples: 'HomePanel', 'GalleryPanel', 'ProductPanel'. " +
                 "Must be consistent across all scenes.")]
        [SerializeField] private string _screenName = "Unnamed Screen";

        // ────────────────────────────────────────────────────────────────────────
        // Runtime state
        // ────────────────────────────────────────────────────────────────────────

        private float _enterTime;
        private bool  _tracking;

        // ────────────────────────────────────────────────────────────────────────
        // Unity lifecycle
        // ────────────────────────────────────────────────────────────────────────

        private void Start()
        {
            if (string.IsNullOrWhiteSpace(_screenName))
            {
                DALogger.Warn($"DAScreenTracker on '{gameObject.name}' has no screen name set. " +
                              "Please set it in the Inspector.");
            }
        }

        private void OnEnable()
        {
            if (string.IsNullOrWhiteSpace(_screenName)) return;
            if (DAAnalyticsManager.Instance == null) return;

            // Record visit
            DAAnalyticsManager.Instance.RecordScreenVisit(_screenName);

            // Start duration timer
            _enterTime = Time.realtimeSinceStartup;
            _tracking  = true;
        }

        private void OnDisable()
        {
            if (!_tracking) return;
            if (string.IsNullOrWhiteSpace(_screenName)) return;
            if (DAAnalyticsManager.Instance == null) return;

            float duration = Time.realtimeSinceStartup - _enterTime;
            _tracking      = false;

            if (duration > 0f)
                DAAnalyticsManager.Instance.RecordScreenDuration(_screenName, duration);
        }

        // ────────────────────────────────────────────────────────────────────────
        // Public API
        // ────────────────────────────────────────────────────────────────────────

        /// <summary>The configured screen name. Read-only at runtime.</summary>
        public string ScreenName => _screenName;
    }
}
