// ============================================================
// DataAnalytics v1.0.0
// DAIdleAnalytics.cs
// Stores aggregate idle time for the current tracking week.
// ============================================================

using System;

namespace DataAnalytics.Runtime.Data
{
    /// <summary>
    /// Stores the total cumulative idle time (in seconds) recorded across the
    /// entire current tracking week. One instance lives inside
    /// <see cref="DAAnalyticsData.idle"/>.
    /// </summary>
    [Serializable]
    public class DAIdleAnalytics
    {
        /// <summary>
        /// Total number of seconds the kiosk has been in an idle state
        /// (no touch, mouse, or keyboard input) during the current week.
        /// Convert to HH:mm:ss during report generation.
        /// </summary>
        public float totalIdleSeconds = 0f;
    }
}
