// ============================================================
// DataAnalytics v1.0.0
// DATimeUtility.cs
// Date/time helpers for IST scheduling and report formatting.
// ============================================================

using System;
using DataAnalytics.Runtime.Utilities;

namespace DataAnalytics.Runtime.Utilities
{
    /// <summary>
    /// Date and time utility helpers for the DataAnalytics package.
    /// Handles timezone conversion, week-start calculation, and duration formatting.
    /// </summary>
    public static class DATimeUtility
    {
        // ────────────────────────────────────────────────────────────────────────
        // Timezone helpers
        // ────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns the current local time converted to the configured timezone
        /// (default: "India Standard Time"). Falls back to <see cref="DateTime.Now"/>
        /// if the timezone ID is invalid.
        /// </summary>
        public static DateTime GetNowInConfiguredZone()
        {
            try
            {
                DASettings settings = DASettingsLoader.Settings;
                string tzId = settings != null ? settings.Timezone : "India Standard Time";
                TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById(tzId);
                return TimeZoneInfo.ConvertTime(DateTime.Now, tz);
            }
            catch (Exception ex)
            {
                DALogger.Warn($"Timezone conversion failed, falling back to local time. {ex.Message}");
                return DateTime.Now;
            }
        }

        // ────────────────────────────────────────────────────────────────────────
        // Week helpers
        // ────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns the ISO 8601 date string (yyyy-MM-dd) of the Monday
        /// that starts the week containing the given date.
        /// </summary>
        /// <param name="date">Any date within the target week.</param>
        public static string GetWeekStartDate(DateTime date)
        {
            int daysSinceMonday = ((int)date.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
            DateTime monday = date.AddDays(-daysSinceMonday).Date;
            return monday.ToString(DAConstants.DATE_FORMAT_ISO);
        }

        /// <summary>
        /// Returns the week-start Monday date string for the current week
        /// in the configured timezone.
        /// </summary>
        public static string GetCurrentWeekStart() =>
            GetWeekStartDate(GetNowInConfiguredZone());

        // ────────────────────────────────────────────────────────────────────────
        // Duration formatting
        // ────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Converts a duration in seconds to a human-readable <c>HH:mm:ss</c> string.
        /// </summary>
        /// <param name="seconds">Duration in seconds (may be fractional).</param>
        public static string FormatSeconds(float seconds)
        {
            if (seconds < 0f) seconds = 0f;
            TimeSpan ts = TimeSpan.FromSeconds(seconds);
            return $"{(int)ts.TotalHours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
        }

        // ────────────────────────────────────────────────────────────────────────
        // Report scheduling
        // ────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns <c>true</c> if the current moment (in the configured timezone)
        /// matches the configured report day, hour, and minute.
        /// Used by <c>DAAnalyticsScheduler</c> to decide when to generate the report.
        /// </summary>
        public static bool IsReportTime()
        {
            DASettings settings = DASettingsLoader.Settings;
            if (settings == null) return false;

            DateTime now = GetNowInConfiguredZone();
            return now.DayOfWeek == settings.ReportDay
                && now.Hour      == settings.ReportHour
                && now.Minute    == settings.ReportMinute;
        }

        /// <summary>
        /// Returns a filename-safe timestamp string for use in report filenames.
        /// Format: <c>yyyy_MM_dd</c>.
        /// </summary>
        public static string GetFilenameTimestamp() =>
            GetNowInConfiguredZone().ToString(DAConstants.DATE_FORMAT_FILENAME);

        /// <summary>
        /// Returns a full timestamp string for metadata fields.
        /// Format: <c>yyyy-MM-dd HH:mm:ss</c>.
        /// </summary>
        public static string GetTimestamp() =>
            DateTime.Now.ToString(DAConstants.DATE_FORMAT_TIMESTAMP);
    }
}
