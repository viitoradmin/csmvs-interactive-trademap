// ============================================================
// DataAnalytics v1.0.0
// DAIdleTimeTracker.cs
// Singleton that detects user inactivity on kiosk touch screens.
// ============================================================

using UnityEngine;
using DataAnalytics.Runtime.Utilities;
using DataAnalytics.Runtime.Managers;

namespace DataAnalytics.Runtime.Managers
{
    /// <summary>
    /// Singleton MonoBehaviour that monitors mouse, touch, and keyboard input
    /// to detect idle periods on Windows kiosk touch-screen applications.
    ///
    /// <para>When no input is detected for <see cref="DASettings.IdleTimeoutSeconds"/>,
    /// idle tracking begins. When input resumes, the elapsed idle duration is recorded
    /// via <see cref="DAAnalyticsManager.RecordIdleTime"/>.</para>
    ///
    /// <para>Survives scene loads via <see cref="DontDestroyOnLoad"/>. Only one
    /// instance should exist at a time; duplicates are destroyed automatically.</para>
    /// </summary>
    public class DAIdleTimeTracker : MonoBehaviour
    {
        // ────────────────────────────────────────────────────────────────────────
        // Singleton
        // ────────────────────────────────────────────────────────────────────────

        private static DAIdleTimeTracker _instance;

        /// <summary>Global singleton instance.</summary>
        public static DAIdleTimeTracker Instance => _instance;

        // ────────────────────────────────────────────────────────────────────────
        // Inspector
        // ────────────────────────────────────────────────────────────────────────

        [Header("Idle Detection Settings")]
        [Space(4)]

        [Tooltip("Seconds of no input before the tracker considers the user idle. Overridden by DASettings.IdleTimeoutSeconds at runtime.")]
        [SerializeField] private float _idleTimeoutSeconds = 60f;

        // ────────────────────────────────────────────────────────────────────────
        // Runtime state
        // ────────────────────────────────────────────────────────────────────────

        private float       _timeSinceLastInput;
        private bool        _isIdle;
        private float       _idleStartTime;
        private Vector3     _lastMousePosition;

        /// <summary>
        /// Returns <c>true</c> when the kiosk is currently in an idle state.
        /// </summary>
        public bool IsIdle => _isIdle;

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
            // Read timeout from settings if available
            DASettings settings = DASettingsLoader.Settings;
            if (settings != null)
                _idleTimeoutSeconds = settings.IdleTimeoutSeconds;

            _lastMousePosition  = Input.mousePosition;
            _timeSinceLastInput = 0f;
            _isIdle             = false;
        }

        private void Update()
        {
            if (HasInputOccurred())
            {
                _timeSinceLastInput = 0f;

                if (_isIdle)
                {
                    // Idle period just ended
                    float idleDuration = Time.realtimeSinceStartup - _idleStartTime;
                    _isIdle            = false;
                    DAAnalyticsManager.Instance.RecordIdleTime(idleDuration);
                }
            }
            else
            {
                _timeSinceLastInput += Time.unscaledDeltaTime;

                if (!_isIdle && _timeSinceLastInput >= _idleTimeoutSeconds)
                {
                    _isIdle        = true;
                    _idleStartTime = Time.realtimeSinceStartup;
                }
            }
        }

        // ────────────────────────────────────────────────────────────────────────
        // Input detection
        // ────────────────────────────────────────────────────────────────────────

        private bool HasInputOccurred()
        {
            // Keyboard
            if (Input.anyKey) return true;

            // Touch (Windows touch-screen kiosks)
            if (Input.touchCount > 0) return true;

            // Mouse movement
            Vector3 currentMouse = Input.mousePosition;
            if (currentMouse != _lastMousePosition)
            {
                _lastMousePosition = currentMouse;
                return true;
            }

            // Mouse buttons
            if (Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2))
                return true;

            return false;
        }
    }
}
