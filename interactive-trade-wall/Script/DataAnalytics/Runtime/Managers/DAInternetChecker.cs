// ============================================================
// DataAnalytics v1.0.0
// DAInternetChecker.cs
// Polls internet connectivity and fires an event when restored.
// ============================================================

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using DataAnalytics.Runtime.Utilities;

namespace DataAnalytics.Runtime.Managers
{
    /// <summary>
    /// Singleton MonoBehaviour that periodically checks internet connectivity
    /// using <see cref="Application.internetReachability"/> and an optional
    /// HTTP HEAD ping. Fires <see cref="OnInternetRestored"/> when the connection
    /// returns after an outage so queued reports can be processed.
    ///
    /// <para>Survives scene loads via <see cref="DontDestroyOnLoad"/>.</para>
    /// </summary>
    public class DAInternetChecker : MonoBehaviour
    {
        // ────────────────────────────────────────────────────────────────────────
        // Singleton
        // ────────────────────────────────────────────────────────────────────────

        private static DAInternetChecker _instance;

        /// <summary>Global singleton instance.</summary>
        public static DAInternetChecker Instance => _instance;

        // ────────────────────────────────────────────────────────────────────────
        // Events
        // ────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Fired when internet connectivity is detected after a period of being offline.
        /// Subscribe to trigger queued report processing.
        /// </summary>
        public static event Action OnInternetRestored;

        // ────────────────────────────────────────────────────────────────────────
        // Runtime state
        // ────────────────────────────────────────────────────────────────────────

        private bool      _wasConnected;
        private bool      _isConnected;
        private Coroutine _checkCoroutine;

        /// <summary>
        /// Returns <c>true</c> if the last connectivity check confirmed internet access.
        /// </summary>
        public bool IsConnected => _isConnected;

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
        }

        private void Start()
        {
            // Assume initial state to avoid false "restored" on startup
            _wasConnected = Application.internetReachability != NetworkReachability.NotReachable;
            _isConnected  = _wasConnected;
            StartChecking();
        }

        private void OnDestroy()
        {
            if (_checkCoroutine != null)
                StopCoroutine(_checkCoroutine);
        }

        // ────────────────────────────────────────────────────────────────────────
        // Check loop
        // ────────────────────────────────────────────────────────────────────────

        private void StartChecking()
        {
            if (_checkCoroutine != null)
                StopCoroutine(_checkCoroutine);

            _checkCoroutine = StartCoroutine(CheckRoutine());
        }

        private IEnumerator CheckRoutine()
        {
            DASettings settings  = DASettingsLoader.Settings;
            float intervalMinutes = settings != null ? settings.InternetCheckIntervalMinutes : 10f;
            float intervalSeconds = intervalMinutes * 60f;

            while (true)
            {
                yield return new WaitForSecondsRealtime(intervalSeconds);
                yield return StartCoroutine(PerformCheck());
            }
        }

        private IEnumerator PerformCheck()
        {
            DASettings settings = DASettingsLoader.Settings;
            bool reachable      = Application.internetReachability != NetworkReachability.NotReachable;

            if (reachable && settings != null && settings.EnableHttpPing)
            {
                // Confirm real connectivity with an HTTP HEAD request
                using (UnityWebRequest request = UnityWebRequest.Head(settings.HttpPingUrl))
                {
                    request.timeout = 5;
                    yield return request.SendWebRequest();
                    reachable = request.result == UnityWebRequest.Result.Success;
                }
            }

            _wasConnected = _isConnected;
            _isConnected  = reachable;

            if (_isConnected && !_wasConnected)
            {
                OnInternetRestored?.Invoke();
            }
        }

        // ────────────────────────────────────────────────────────────────────────
        // Public API
        // ────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Triggers an immediate connectivity check outside of the normal schedule.
        /// Useful for on-demand checks before attempting to send a report.
        /// </summary>
        public void CheckNow()
        {
            StartCoroutine(PerformCheck());
        }
    }
}
