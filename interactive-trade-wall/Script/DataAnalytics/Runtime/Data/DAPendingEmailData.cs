// ============================================================
// DataAnalytics v1.0.0
// DAPendingEmailData.cs
// Describes a single queued report awaiting email delivery.
// ============================================================

using System;
using DataAnalytics.Runtime.Utilities;

namespace DataAnalytics.Runtime.Data
{
    /// <summary>
    /// Represents a single entry in the email queue.
    /// Persisted to <c>email_queue.json</c> so that pending reports survive
    /// application restarts and are retried when internet connectivity returns.
    /// </summary>
    [Serializable]
    public class DAPendingEmailData
    {
        /// <summary>
        /// ISO 8601 week-start date string identifying which week this report covers
        /// (e.g. "2024-01-08" for the week starting Monday 8 January 2024).
        /// </summary>
        public string reportWeek = string.Empty;

        /// <summary>
        /// Absolute file-system path to the generated CSV report file.
        /// Stored so the email service can attach it without searching the disk.
        /// </summary>
        public string excelPath = string.Empty;

        /// <summary>
        /// Current delivery status of this report.
        /// Valid values: <see cref="DAConstants.EMAIL_STATUS_PENDING"/>,
        /// <see cref="DAConstants.EMAIL_STATUS_SENT"/>,
        /// <see cref="DAConstants.EMAIL_STATUS_FAILED"/>.
        /// </summary>
        public string status = DAConstants.EMAIL_STATUS_PENDING;

        /// <summary>
        /// Timestamp (local time) at which this entry was added to the queue.
        /// </summary>
        public string queuedAt = string.Empty;

        // ────────────────────────────────────────────────────────────────────────
        // Factory
        // ────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Creates a new pending email entry with the current timestamp.
        /// </summary>
        /// <param name="weekStart">ISO 8601 Monday date of the report week.</param>
        /// <param name="reportFilePath">Absolute path to the generated CSV file.</param>
        public static DAPendingEmailData CreateNew(string weekStart, string reportFilePath)
        {
            return new DAPendingEmailData
            {
                reportWeek = weekStart,
                excelPath  = reportFilePath,
                status     = DAConstants.EMAIL_STATUS_PENDING,
                queuedAt   = DateTime.Now.ToString(DAConstants.DATE_FORMAT_TIMESTAMP)
            };
        }
    }
}
