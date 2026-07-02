// ============================================================
// DataAnalytics v1.0.0
// DAConstants.cs
// All magic strings centralized — no hardcoded values elsewhere.
// ============================================================

namespace DataAnalytics.Runtime.Utilities
{
    /// <summary>
    /// Central repository for all constant strings and values used across
    /// the DataAnalytics package. Never use raw strings outside this class.
    /// </summary>
    public static class DAConstants
    {
        // ────────────────────────────────────────────────────────────────────────
        // Logging
        // ────────────────────────────────────────────────────────────────────────

        /// <summary>Prefix applied to every log message emitted by DataAnalytics.</summary>
        public const string LOG_PREFIX = "[DataAnalytics]";

        // ────────────────────────────────────────────────────────────────────────
        // Root folder (inside Application.persistentDataPath)
        // ────────────────────────────────────────────────────────────────────────

        /// <summary>Root folder name inside Application.persistentDataPath.</summary>
        public const string ROOT_FOLDER = "DataAnalytics";

        /// <summary>Sub-folder for archived weekly JSON files.</summary>
        public const string ARCHIVE_FOLDER = "Archive";

        /// <summary>Sub-folder for generated CSV/Excel reports.</summary>
        public const string REPORTS_FOLDER = "Reports";

        /// <summary>Sub-folder for reports that could not be sent immediately.</summary>
        public const string PENDING_REPORTS_FOLDER = "PendingReports";

        /// <summary>Sub-folder for the email queue JSON file.</summary>
        public const string QUEUE_FOLDER = "Queue";

        // ────────────────────────────────────────────────────────────────────────
        // File names
        // ────────────────────────────────────────────────────────────────────────

        /// <summary>Live analytics JSON file name.</summary>
        public const string ANALYTICS_CURRENT_FILE = "analytics_current.json";

        /// <summary>Email queue JSON file name.</summary>
        public const string EMAIL_QUEUE_FILE = "email_queue.json";

        /// <summary>Weekly report filename prefix (date is appended).</summary>
        public const string REPORT_FILE_PREFIX = "WeeklyReport_";

        /// <summary>Weekly report file extension (Phase 1: CSV).</summary>
        public const string REPORT_FILE_EXTENSION = ".csv";

        // ────────────────────────────────────────────────────────────────────────
        // Resources
        // ────────────────────────────────────────────────────────────────────────

        /// <summary>Path used with Resources.Load to fetch DASettings asset.</summary>
        public const string SETTINGS_RESOURCE_PATH = "DASettings";

        // ────────────────────────────────────────────────────────────────────────
        // Email status strings
        // ────────────────────────────────────────────────────────────────────────

        /// <summary>Status string for a report that has not yet been sent.</summary>
        public const string EMAIL_STATUS_PENDING = "Pending";

        /// <summary>Status string for a report that was successfully sent.</summary>
        public const string EMAIL_STATUS_SENT = "Sent";

        /// <summary>Status string for a report that failed to send.</summary>
        public const string EMAIL_STATUS_FAILED = "Failed";

        // ────────────────────────────────────────────────────────────────────────
        // Log messages
        // ────────────────────────────────────────────────────────────────────────

        public const string MSG_LOADED          = "Analytics loaded.";
        public const string MSG_AUTOSAVE        = "Autosave completed.";
        public const string MSG_PENDING_REPORT  = "Pending report detected.";
        public const string MSG_INTERNET_RESTORED = "Internet restored.";
        public const string MSG_REPORT_GENERATED = "Weekly report generated.";
        public const string MSG_IDLE_STARTED    = "Idle period started.";
        public const string MSG_IDLE_ENDED      = "Idle period ended.";
        public const string MSG_DIRECTORIES_OK  = "Storage directories verified.";
        public const string MSG_EMAIL_PHASE2    = "EMAIL PHASE 2: Not implemented yet.";

        public const string MSG_UPLOAD_SUCCESS  = "Report uploaded to backend.";
        public const string MSG_UPLOAD_FAILED   = "Report upload failed.";
        public const string MSG_UPLOAD_SKIPPED  = "Report upload skipped — BackendUploadUrl or UploadApiKey not configured.";

        // ────────────────────────────────────────────────────────────────────────
        // Backend report upload (multipart/form-data)
        // ────────────────────────────────────────────────────────────────────────

        /// <summary>Multipart field name for the report file.</summary>
        public const string UPLOAD_FIELD_FILE     = "file";

        /// <summary>Multipart field name for the ISO week-start date.</summary>
        public const string UPLOAD_FIELD_WEEK     = "week";

        /// <summary>Multipart field name for the kiosk device identifier (dedupe).</summary>
        public const string UPLOAD_FIELD_DEVICE   = "device_id";

        /// <summary>Multipart field name for the human-readable app name.</summary>
        public const string UPLOAD_FIELD_APP_NAME = "app_name";

        /// <summary>Header carrying the shared backend API key.</summary>
        public const string UPLOAD_HEADER_API_KEY = "X-API-Key";

        /// <summary>MIME type for the uploaded CSV report.</summary>
        public const string UPLOAD_CSV_MIME       = "text/csv";

        // ────────────────────────────────────────────────────────────────────────
        // CSV report section headers
        // ────────────────────────────────────────────────────────────────────────

        public const string CSV_SECTION_PRODUCTS  = "=== PRODUCT ANALYTICS ===";
        public const string CSV_SECTION_LANGUAGES = "=== LANGUAGE ANALYTICS ===";
        public const string CSV_SECTION_SCREENS   = "=== SCREEN ANALYTICS ===";
        public const string CSV_SECTION_IDLE      = "=== IDLE ANALYTICS ===";
        public const string CSV_SECTION_META      = "=== REPORT METADATA ===";

        // ────────────────────────────────────────────────────────────────────────
        // Date / time
        // ────────────────────────────────────────────────────────────────────────

        /// <summary>ISO 8601 date format used throughout storage and archives.</summary>
        public const string DATE_FORMAT_ISO       = "yyyy-MM-dd";

        /// <summary>Full date-time format used in filenames and report metadata.</summary>
        public const string DATE_FORMAT_FILENAME  = "yyyy_MM_dd";

        /// <summary>Full date-time format used in JSON timestamps.</summary>
        public const string DATE_FORMAT_TIMESTAMP = "yyyy-MM-dd HH:mm:ss";
    }
}
