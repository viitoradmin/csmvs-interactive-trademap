// ============================================================
// DataAnalytics v1.0.0
// DALanguageAnalytics.cs
// Tracks selection count and reading duration per language.
// ============================================================

using System;

namespace DataAnalytics.Runtime.Data
{
    /// <summary>
    /// Stores how many times a language toggle was selected and the total
    /// cumulative reading duration (in seconds) for that language during
    /// the current tracking week.
    /// </summary>
    [Serializable]
    public class DALanguageAnalytics
    {
        /// <summary>
        /// Unique display name of the language (e.g. "English", "Marathi").
        /// Must exactly match the <c>languageName</c> field set on the
        /// <c>DALanguageTracker</c> component.
        /// </summary>
        public string languageName = string.Empty;

        /// <summary>
        /// Number of times this language toggle was turned ON (selected)
        /// during the current tracking week.
        /// </summary>
        public int selectionCount = 0;

        /// <summary>
        /// Cumulative reading time in seconds that the user spent with this
        /// language selected. Convert to HH:mm:ss during report generation.
        /// </summary>
        public float totalSeconds = 0f;
    }
}
