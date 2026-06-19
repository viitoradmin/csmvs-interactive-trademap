// ============================================================
// DataAnalytics v1.0.0
// DAAnalyticsData.cs
// Root serializable data container for all analytics categories.
// ============================================================

using System;
using System.Collections.Generic;
using DataAnalytics.Runtime.Utilities;

namespace DataAnalytics.Runtime.Data
{
    /// <summary>
    /// Root data object serialized to <c>analytics_current.json</c>.
    /// Contains all analytics sub-categories for a single tracking week.
    /// Serialized with Unity's built-in JsonUtility.
    /// </summary>
    [Serializable]
    public class DAAnalyticsData
    {
        // ────────────────────────────────────────────────────────────────────────
        // Metadata
        // ────────────────────────────────────────────────────────────────────────

        /// <summary>ISO 8601 date string of the Monday that starts this tracking week.</summary>
        public string weekStartDate = string.Empty;

        /// <summary>Timestamp of the last time this data was saved to disk.</summary>
        public string lastSavedAt = string.Empty;

        // ────────────────────────────────────────────────────────────────────────
        // Analytics sub-categories
        // ────────────────────────────────────────────────────────────────────────

        /// <summary>Per-product click count analytics.</summary>
        public List<DAProductAnalytics> products = new List<DAProductAnalytics>();

        /// <summary>Per-language selection count and reading duration analytics.</summary>
        public List<DALanguageAnalytics> languages = new List<DALanguageAnalytics>();

        /// <summary>Per-screen visit count and total time spent analytics.</summary>
        public List<DAScreenAnalytics> screens = new List<DAScreenAnalytics>();

        /// <summary>Aggregate idle time analytics for the week.</summary>
        public DAIdleAnalytics idle = new DAIdleAnalytics();

        // ────────────────────────────────────────────────────────────────────────
        // Factory
        // ────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Creates a fresh <see cref="DAAnalyticsData"/> instance stamped with
        /// the supplied week-start date.
        /// </summary>
        /// <param name="weekStart">ISO 8601 Monday date string.</param>
        public static DAAnalyticsData CreateNew(string weekStart)
        {
            return new DAAnalyticsData
            {
                weekStartDate = weekStart,
                lastSavedAt   = DateTime.Now.ToString(DAConstants.DATE_FORMAT_TIMESTAMP),
                products      = new List<DAProductAnalytics>(),
                languages     = new List<DALanguageAnalytics>(),
                screens       = new List<DAScreenAnalytics>(),
                idle          = new DAIdleAnalytics()
            };
        }
    }
}
