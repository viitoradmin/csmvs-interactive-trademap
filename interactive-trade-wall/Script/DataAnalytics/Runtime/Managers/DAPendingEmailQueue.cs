// ============================================================
// DataAnalytics v1.0.0
// DAPendingEmailQueue.cs
// Manages the persistent queue of reports awaiting email delivery.
// ============================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using DataAnalytics.Runtime.Data;
using DataAnalytics.Runtime.Storage;
using DataAnalytics.Runtime.Utilities;

namespace DataAnalytics.Runtime.Managers
{
    /// <summary>
    /// Singleton MonoBehaviour that manages the persistent email queue.
    /// Reports that could not be sent immediately (due to no internet) are
    /// added to the queue and retried in chronological order when connectivity
    /// is restored.
    ///
    /// <para>Survives scene loads via <see cref="DontDestroyOnLoad"/>.</para>
    /// </summary>
    public class DAPendingEmailQueue : MonoBehaviour
    {
        // ────────────────────────────────────────────────────────────────────────
        // Singleton
        // ────────────────────────────────────────────────────────────────────────

        private static DAPendingEmailQueue _instance;

        /// <summary>Global singleton instance.</summary>
        public static DAPendingEmailQueue Instance => _instance;

        // ────────────────────────────────────────────────────────────────────────
        // Runtime state
        // ────────────────────────────────────────────────────────────────────────

        private List<DAPendingEmailData> _queue = new List<DAPendingEmailData>();

        /// <summary>Number of reports currently in the pending queue.</summary>
        public int PendingCount => _queue.Count;

        // ────────────────────────────────────────────────────────────────────────
        // Unity lifecycle
        // ────────────────────────────────────────────────────────────────────────

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            LoadQueue();

            // Subscribe to internet restoration event
            DAInternetChecker.OnInternetRestored += OnInternetRestored;

            if (_queue.Count > 0)
                DALogger.Log($"{DAConstants.MSG_PENDING_REPORT} {_queue.Count} report(s) queued.");
        }

        private void OnDestroy()
        {
            DAInternetChecker.OnInternetRestored -= OnInternetRestored;
        }

        // ────────────────────────────────────────────────────────────────────────
        // Queue management
        // ────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Adds a new report to the end of the pending queue and immediately
        /// persists the updated queue to disk.
        /// </summary>
        /// <param name="entry">The report entry to enqueue.</param>
        public void EnqueueReport(DAPendingEmailData entry)
        {
            if (entry == null)
            {
                DALogger.Warn("EnqueueReport called with null entry — skipped.");
                return;
            }

            _queue.Add(entry);
            SaveQueue();
            DALogger.Log($"Report enqueued: week {entry.reportWeek} (total queued: {_queue.Count}).");
        }

        /// <summary>
        /// Processes all pending reports in chronological order (oldest first).
        /// In Phase 1 this logs a stub message; Phase 2 will invoke <c>DAEmailService</c>.
        /// </summary>
        public void ProcessQueue()
        {
            if (_queue.Count == 0)
            {
                DALogger.Log("ProcessQueue: no pending reports.");
                return;
            }

            DALogger.Log($"ProcessQueue: attempting to send {_queue.Count} pending report(s).");

            // Process oldest-first (queue is already in insertion order)
            foreach (var entry in _queue)
            {
                if (entry.status == DAConstants.EMAIL_STATUS_SENT)
                    continue;

                // PHASE 2: Replace this block with DAEmailService.SendReportAsync(entry)
                DALogger.Log($"{DAConstants.MSG_EMAIL_PHASE2} Report: {entry.reportWeek}");

                // For now, mark as "sent" so the queue doesn't grow indefinitely.
                // In Phase 2 this will only be set after confirmed delivery.
                entry.status = DAConstants.EMAIL_STATUS_SENT;
            }

            SaveQueue();
        }

        // ────────────────────────────────────────────────────────────────────────
        // Persistence
        // ────────────────────────────────────────────────────────────────────────

        private void LoadQueue()
        {
            try
            {
                _queue = DAStorageManager.LoadPendingQueue();
            }
            catch (Exception ex)
            {
                DALogger.Exception("DAPendingEmailQueue.LoadQueue", ex);
                _queue = new List<DAPendingEmailData>();
            }
        }

        private void SaveQueue()
        {
            try
            {
                DAStorageManager.SavePendingQueue(_queue);
            }
            catch (Exception ex)
            {
                DALogger.Exception("DAPendingEmailQueue.SaveQueue", ex);
            }
        }

        // ────────────────────────────────────────────────────────────────────────
        // Event handlers
        // ────────────────────────────────────────────────────────────────────────

        private void OnInternetRestored()
        {
            if (_queue.Count > 0)
                ProcessQueue();
        }
    }
}
