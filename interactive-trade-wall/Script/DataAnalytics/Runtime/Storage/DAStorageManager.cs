// ============================================================
// DataAnalytics v1.0.0
// DAStorageManager.cs
// Handles all disk I/O for the DataAnalytics package.
// All paths derive from Application.persistentDataPath — never hardcoded.
// ============================================================

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using DataAnalytics.Runtime.Data;
using DataAnalytics.Runtime.Utilities;

namespace DataAnalytics.Runtime.Storage
{
    /// <summary>
    /// Static utility responsible for all file-system operations used by the
    /// DataAnalytics package. Creates required directories, serializes and
    /// deserializes analytics JSON, and archives completed weekly data.
    ///
    /// <para>Storage structure (inside <see cref="Application.persistentDataPath"/>):</para>
    /// <code>
    /// DataAnalytics/
    ///   analytics_current.json
    ///   Archive/
    ///   Reports/
    ///   PendingReports/
    ///   Queue/
    ///     email_queue.json
    /// </code>
    /// </summary>
    public static class DAStorageManager
    {
        // ────────────────────────────────────────────────────────────────────────
        // Path properties
        // ────────────────────────────────────────────────────────────────────────

        /// <summary>Root DataAnalytics folder inside Application.persistentDataPath.</summary>
        public static string RootPath =>
            Path.Combine(Application.persistentDataPath, DAConstants.ROOT_FOLDER);

        /// <summary>Path to the live analytics JSON file.</summary>
        public static string CurrentAnalyticsPath =>
            Path.Combine(RootPath, DAConstants.ANALYTICS_CURRENT_FILE);

        /// <summary>Path to the Archive sub-folder.</summary>
        public static string ArchivePath =>
            Path.Combine(RootPath, DAConstants.ARCHIVE_FOLDER);

        /// <summary>Path to the Reports sub-folder.</summary>
        public static string ReportsPath =>
            Path.Combine(RootPath, DAConstants.REPORTS_FOLDER);

        /// <summary>Path to the PendingReports sub-folder.</summary>
        public static string PendingReportsPath =>
            Path.Combine(RootPath, DAConstants.PENDING_REPORTS_FOLDER);

        /// <summary>Path to the Queue sub-folder.</summary>
        public static string QueuePath =>
            Path.Combine(RootPath, DAConstants.QUEUE_FOLDER);

        /// <summary>Path to the email queue JSON file.</summary>
        public static string EmailQueuePath =>
            Path.Combine(QueuePath, DAConstants.EMAIL_QUEUE_FILE);

        // ────────────────────────────────────────────────────────────────────────
        // Directory setup
        // ────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Creates all required DataAnalytics directories if they do not already exist.
        /// Call this once on application start before any other storage operations.
        /// </summary>
        public static void EnsureDirectories()
        {
            try
            {
                Directory.CreateDirectory(RootPath);
                Directory.CreateDirectory(ArchivePath);
                Directory.CreateDirectory(ReportsPath);
                Directory.CreateDirectory(PendingReportsPath);
                Directory.CreateDirectory(QueuePath);
            }
            catch (Exception ex)
            {
                DALogger.Exception("EnsureDirectories", ex);
            }
        }

        // ────────────────────────────────────────────────────────────────────────
        // Analytics JSON — Save
        // ────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Serializes the supplied <see cref="DAAnalyticsData"/> to
        /// <c>analytics_current.json</c>. Overwrites the existing file.
        /// Updates <see cref="DAAnalyticsData.lastSavedAt"/> before writing.
        /// </summary>
        /// <param name="data">Analytics data to persist.</param>
        public static void SaveCurrentAnalytics(DAAnalyticsData data)
        {
            if (data == null)
            {
                DALogger.Warn("SaveCurrentAnalytics called with null data — skipped.");
                return;
            }

            try
            {
                data.lastSavedAt = DATimeUtility.GetTimestamp();
                string json = JsonUtility.ToJson(data, prettyPrint: true);
                File.WriteAllText(CurrentAnalyticsPath, json);
            }
            catch (Exception ex)
            {
                DALogger.Exception("SaveCurrentAnalytics", ex);
            }
        }

        // ────────────────────────────────────────────────────────────────────────
        // Analytics JSON — Load
        // ────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Loads and deserializes <c>analytics_current.json</c>.
        /// Returns <c>null</c> if the file does not exist (caller must handle).
        /// Returns <c>null</c> and logs an error if deserialization fails.
        /// </summary>
        public static DAAnalyticsData LoadCurrentAnalytics()
        {
            try
            {
                if (!File.Exists(CurrentAnalyticsPath))
                    return null;

                string json = File.ReadAllText(CurrentAnalyticsPath);
                DAAnalyticsData data = JsonUtility.FromJson<DAAnalyticsData>(json);
                return data;
            }
            catch (Exception ex)
            {
                DALogger.Exception("LoadCurrentAnalytics", ex);
                return null;
            }
        }

        // ────────────────────────────────────────────────────────────────────────
        // Archive
        // ────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Moves <c>analytics_current.json</c> to the Archive folder, renamed with
        /// the supplied week label so it is never overwritten.
        /// </summary>
        /// <param name="weekLabel">
        /// Human-readable week identifier (e.g. "2024-01-08") used in the archived filename.
        /// </param>
        public static void ArchiveCurrentFile(string weekLabel)
        {
            try
            {
                if (!File.Exists(CurrentAnalyticsPath))
                {
                    DALogger.Warn("ArchiveCurrentFile: analytics_current.json not found — nothing to archive.");
                    return;
                }

                string archiveFileName = $"analytics_{weekLabel}.json";
                string destination     = Path.Combine(ArchivePath, archiveFileName);

                // Never overwrite archives — append a counter if needed.
                int counter = 1;
                while (File.Exists(destination))
                {
                    archiveFileName = $"analytics_{weekLabel}_{counter++}.json";
                    destination     = Path.Combine(ArchivePath, archiveFileName);
                }

                File.Move(CurrentAnalyticsPath, destination);
            }
            catch (Exception ex)
            {
                DALogger.Exception("ArchiveCurrentFile", ex);
            }
        }

        // ────────────────────────────────────────────────────────────────────────
        // Email queue — Save
        // ────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Wrapper list used for JSON serialization of <see cref="DAPendingEmailData"/> lists,
        /// because <see cref="JsonUtility"/> requires a class wrapper for top-level arrays.
        /// </summary>
        [Serializable]
        private class EmailQueueWrapper
        {
            public List<DAPendingEmailData> queue = new List<DAPendingEmailData>();
        }

        /// <summary>
        /// Persists the supplied email queue list to <c>email_queue.json</c>.
        /// </summary>
        /// <param name="queue">Current queue of pending email entries.</param>
        public static void SavePendingQueue(List<DAPendingEmailData> queue)
        {
            if (queue == null) queue = new List<DAPendingEmailData>();

            try
            {
                EmailQueueWrapper wrapper = new EmailQueueWrapper { queue = queue };
                string json = JsonUtility.ToJson(wrapper, prettyPrint: true);
                File.WriteAllText(EmailQueuePath, json);
            }
            catch (Exception ex)
            {
                DALogger.Exception("SavePendingQueue", ex);
            }
        }

        // ────────────────────────────────────────────────────────────────────────
        // Email queue — Load
        // ────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Loads and deserializes <c>email_queue.json</c>.
        /// Returns an empty list if the file does not exist.
        /// </summary>
        public static List<DAPendingEmailData> LoadPendingQueue()
        {
            try
            {
                if (!File.Exists(EmailQueuePath))
                    return new List<DAPendingEmailData>();

                string json        = File.ReadAllText(EmailQueuePath);
                EmailQueueWrapper wrapper = JsonUtility.FromJson<EmailQueueWrapper>(json);
                return wrapper?.queue ?? new List<DAPendingEmailData>();
            }
            catch (Exception ex)
            {
                DALogger.Exception("LoadPendingQueue", ex);
                return new List<DAPendingEmailData>();
            }
        }

        // ────────────────────────────────────────────────────────────────────────
        // Helpers
        // ────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns the full path for a new report file inside the Reports folder.
        /// </summary>
        /// <param name="weekLabel">yyyy_MM_dd date string for the filename.</param>
        public static string BuildReportPath(string weekLabel) =>
            Path.Combine(ReportsPath,
                $"{DAConstants.REPORT_FILE_PREFIX}{weekLabel}{DAConstants.REPORT_FILE_EXTENSION}");

        /// <summary>
        /// Returns the full path for a pending report inside the PendingReports folder.
        /// </summary>
        /// <param name="weekLabel">yyyy_MM_dd date string for the filename.</param>
        public static string BuildPendingReportPath(string weekLabel) =>
            Path.Combine(PendingReportsPath,
                $"{DAConstants.REPORT_FILE_PREFIX}{weekLabel}{DAConstants.REPORT_FILE_EXTENSION}");
    }
}
