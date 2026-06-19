// ============================================================
// DataAnalytics v1.0.0
// DAScreenAnalytics.cs
// Tracks visit count and total time spent per screen/panel.
// ============================================================

using System;

namespace DataAnalytics.Runtime.Data
{
    /// <summary>
    /// Stores how many times a screen (panel/canvas) was visited and the
    /// total cumulative time users spent on it during the current tracking week.
    /// </summary>
    [Serializable]
    public class DAScreenAnalytics
    {
        /// <summary>
        /// Unique display name of the screen (e.g. "HomePanel", "GalleryPanel").
        /// Must exactly match the <c>screenName</c> field set on the
        /// <c>DAScreenTracker</c> component.
        /// </summary>
        public string screenName = string.Empty;

        /// <summary>
        /// Number of times this screen was enabled (navigated to) during
        /// the current tracking week.
        /// </summary>
        public int visitCount = 0;

        /// <summary>
        /// Cumulative time in seconds the screen was active (enabled).
        /// Convert to HH:mm:ss during report generation.
        /// </summary>
        public float totalSeconds = 0f;
    }
}
