// ============================================================
// DataAnalytics v1.0.0
// DASettings.cs
// ScriptableObject configuration — Inspector-first design.
// ============================================================

using System;
using UnityEngine;

namespace DataAnalytics.Runtime
{
    /// <summary>
    /// Central configuration ScriptableObject for the DataAnalytics package.
    /// Create one instance at: Assets/DataAnalytics/Resources/DASettings.asset
    /// and tune all settings from the Inspector — no code changes required.
    /// </summary>
    [CreateAssetMenu(
        fileName = "DASettings",
        menuName  = "DataAnalytics/Settings",
        order     = 0)]
    public class DASettings : ScriptableObject
    {
        // ────────────────────────────────────────────────────────────────────────
        // Save Settings
        // ────────────────────────────────────────────────────────────────────────

        [Header("Save Settings")]
        [Space(4)]

        [Tooltip("How often (in seconds) the analytics data is auto-saved to disk. Default: 30 seconds.")]
        [SerializeField] private float _saveIntervalSeconds = 30f;

        /// <summary>Auto-save interval in seconds.</summary>
        public float SaveIntervalSeconds => _saveIntervalSeconds;

        // ────────────────────────────────────────────────────────────────────────
        // Idle Detection
        // ────────────────────────────────────────────────────────────────────────

        [Header("Idle Detection")]
        [Space(4)]

        [Tooltip("Seconds of no input before the system considers the user idle. Default: 60 seconds.")]
        [SerializeField] private float _idleTimeoutSeconds = 60f;

        /// <summary>Inactivity threshold in seconds before idle state begins.</summary>
        public float IdleTimeoutSeconds => _idleTimeoutSeconds;

        // ────────────────────────────────────────────────────────────────────────
        // Report Schedule
        // ────────────────────────────────────────────────────────────────────────

        [Header("Weekly Report Schedule")]
        [Space(4)]

        [Tooltip("Day of the week on which the weekly analytics report is generated. Default: Saturday.")]
        [SerializeField] private DayOfWeek _reportDay = DayOfWeek.Saturday;

        /// <summary>Day of week the report is generated.</summary>
        public DayOfWeek ReportDay => _reportDay;

        [Tooltip("Hour (0–23) at which the report is generated, in the configured timezone. Default: 20 (8 PM IST).")]
        [Range(0, 23)]
        [SerializeField] private int _reportHour = 20;

        /// <summary>Hour (0–23) at which the report is generated.</summary>
        public int ReportHour => _reportHour;

        [Tooltip("Minute (0–59) at which the report is generated. Default: 0.")]
        [Range(0, 59)]
        [SerializeField] private int _reportMinute = 0;

        /// <summary>Minute (0–59) at which the report is generated.</summary>
        public int ReportMinute => _reportMinute;

        // ────────────────────────────────────────────────────────────────────────
        // Internet Check
        // ────────────────────────────────────────────────────────────────────────

        [Header("Internet Connectivity Check")]
        [Space(4)]

        [Tooltip("How often (in minutes) the system checks for internet connectivity. Default: 10 minutes.")]
        [SerializeField] private float _internetCheckIntervalMinutes = 10f;

        /// <summary>Internet connectivity polling interval in minutes.</summary>
        public float InternetCheckIntervalMinutes => _internetCheckIntervalMinutes;

        [Tooltip("Enable an optional HTTP HEAD ping for a more reliable connectivity check (in addition to Application.internetReachability).")]
        [SerializeField] private bool _enableHttpPing = true;

        /// <summary>When true, an HTTP HEAD request is made to confirm real connectivity.</summary>
        public bool EnableHttpPing => _enableHttpPing;

        [Tooltip("URL used for the optional HTTP HEAD ping. Must be reachable from the kiosk network. Default: http://clients3.google.com/generate_204")]
        [SerializeField] private string _httpPingUrl = "http://clients3.google.com/generate_204";

        /// <summary>URL pinged to confirm internet connectivity.</summary>
        public string HttpPingUrl => _httpPingUrl;

        // ────────────────────────────────────────────────────────────────────────
        // Logging
        // ────────────────────────────────────────────────────────────────────────

        [Header("Logging")]
        [Space(4)]

        [Tooltip("Enable or disable all [DataAnalytics] console logs. Disable in production builds to reduce noise.")]
        [SerializeField] private bool _enableLogging = true;

        /// <summary>Master switch for all DataAnalytics console logging.</summary>
        public bool EnableLogging => _enableLogging;

        // ────────────────────────────────────────────────────────────────────────
        // Timezone
        // ────────────────────────────────────────────────────────────────────────

        [Header("Timezone")]
        [Space(4)]

        [Tooltip("Windows timezone ID used for all report scheduling. Default: 'India Standard Time'. Use TimeZoneInfo.GetSystemTimeZones() to find valid IDs.")]
        [SerializeField] private string _timezone = "India Standard Time";

        /// <summary>Windows timezone ID for report scheduling.</summary>
        public string Timezone => _timezone;

        // ────────────────────────────────────────────────────────────────────────
        // Email (Phase 2 — not used in Phase 1)
        // ────────────────────────────────────────────────────────────────────────

        [Header("Email Recipients (Phase 2 — Not Active)")]
        [Space(4)]

        [Tooltip("List of email addresses that will receive weekly reports. PHASE 2 ONLY — not active in this version.")]
        [SerializeField] private string[] _emailRecipients = new string[0];

        /// <summary>Email recipients for weekly reports (Phase 2).</summary>
        public string[] EmailRecipients => _emailRecipients;
    }
}
