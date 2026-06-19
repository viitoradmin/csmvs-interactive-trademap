// ============================================================
// DataAnalytics v1.0.0
// DASettingsLoader.cs
// Lazy-loaded singleton accessor for DASettings ScriptableObject.
// ============================================================

using UnityEngine;
using DataAnalytics.Runtime.Utilities;

namespace DataAnalytics.Runtime.Utilities
{
    /// <summary>
    /// Static accessor that lazy-loads <see cref="DASettings"/> from the
    /// Resources folder on first access. Caches the result for the lifetime
    /// of the application. Logs a warning if the asset is missing.
    /// </summary>
    public static class DASettingsLoader
    {
        private static DASettings _settings;

        /// <summary>
        /// The loaded <see cref="DASettings"/> instance.
        /// Loaded once from <c>Resources/DASettings.asset</c> on first access.
        /// Returns <c>null</c> if the asset does not exist (logs a warning).
        /// </summary>
        public static DASettings Settings
        {
            get
            {
                if (_settings == null)
                {
                    _settings = Resources.Load<DASettings>(DAConstants.SETTINGS_RESOURCE_PATH);

                    if (_settings == null)
                    {
                        Debug.LogWarning(
                            $"{DAConstants.LOG_PREFIX} DASettings.asset not found at " +
                            $"Resources/{DAConstants.SETTINGS_RESOURCE_PATH}. " +
                            "Please create it via Assets > Create > DataAnalytics > Settings.");
                    }
                }

                return _settings;
            }
        }

        /// <summary>
        /// Clears the cached settings instance, forcing a reload on the next access.
        /// Useful in Editor scripts or during testing.
        /// </summary>
        public static void InvalidateCache() => _settings = null;

        /// <summary>
        /// Overrides the active settings with an explicitly supplied asset, bypassing
        /// the Resources lookup. Pass a non-null <see cref="DASettings"/> to use a
        /// specific asset (e.g. assigned in the Inspector); the override is cached for
        /// the lifetime of the application. Passing <c>null</c> clears any override and
        /// reverts to loading from Resources on the next access.
        /// </summary>
        /// <param name="settings">The settings asset to use, or <c>null</c> to revert.</param>
        public static void SetOverride(DASettings settings) => _settings = settings;
    }
}
