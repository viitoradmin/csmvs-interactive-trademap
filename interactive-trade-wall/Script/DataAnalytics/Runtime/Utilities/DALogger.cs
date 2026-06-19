// ============================================================
// DataAnalytics v1.0.0
// DALogger.cs
// Centralized logging with [DataAnalytics] prefix and on/off switch.
// ============================================================

using UnityEngine;
using DataAnalytics.Runtime.Utilities;

namespace DataAnalytics.Runtime.Utilities
{
    /// <summary>
    /// Centralized logging utility for the DataAnalytics package.
    /// All log output is prefixed with <c>[DataAnalytics]</c> and can be
    /// toggled on or off via <see cref="DASettings.EnableLogging"/>.
    /// Use this class instead of calling <see cref="Debug"/> directly.
    /// </summary>
    public static class DALogger
    {
        // ────────────────────────────────────────────────────────────────────────
        // Internal helpers
        // ────────────────────────────────────────────────────────────────────────

        private static bool IsEnabled
        {
            get
            {
                DASettings settings = DASettingsLoader.Settings;
                return settings == null || settings.EnableLogging;
            }
        }

        private static string Format(string message) =>
            $"{DAConstants.LOG_PREFIX} {message}";

        // ────────────────────────────────────────────────────────────────────────
        // Public API
        // ────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Logs an informational message to the Unity console.
        /// </summary>
        /// <param name="message">Message body (without prefix).</param>
        public static void Log(string message)
        {
            if (IsEnabled)
                Debug.Log(Format(message));
        }

        /// <summary>
        /// Logs a warning message to the Unity console.
        /// </summary>
        /// <param name="message">Message body (without prefix).</param>
        public static void Warn(string message)
        {
            if (IsEnabled)
                Debug.LogWarning(Format(message));
        }

        /// <summary>
        /// Logs an error message to the Unity console.
        /// Errors are always logged regardless of the EnableLogging setting.
        /// </summary>
        /// <param name="message">Message body (without prefix).</param>
        public static void Error(string message)
        {
            // Errors always logged — never silenced.
            Debug.LogError(Format(message));
        }

        /// <summary>
        /// Logs an exception to the Unity console.
        /// Always logged regardless of the EnableLogging setting.
        /// </summary>
        /// <param name="context">Human-readable description of where the exception occurred.</param>
        /// <param name="ex">The caught exception.</param>
        public static void Exception(string context, System.Exception ex)
        {
            Debug.LogError(Format($"{context} — Exception: {ex.Message}\n{ex.StackTrace}"));
        }
    }
}
