// ============================================================
// DataAnalytics v1.0.0
// DAPendingEmailQueue.cs
// Manages the persistent queue of reports awaiting email delivery.
// ============================================================

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataAnalytics.Runtime.Data;
using DataAnalytics.Runtime.Network;
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

        /// <summary>Guard against overlapping ProcessQueue runs.</summary>
        private bool _isProcessing;

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
        }

        /// <summary>
        /// Processes all pending reports in chronological order (oldest first).
        /// In Phase 1 this logs a stub message; Phase 2 will invoke <c>DAEmailService</c>.
        /// </summary>
        public void ProcessQueue()
        {
            if (_isProcessing || _queue.Count == 0) return;
            StartCoroutine(ProcessQueueRoutine());
        }

        /// <summary>
        /// Uploads pending reports oldest-first via <see cref="DAReportUploader"/>.
        /// On success an entry is marked Sent; on failure it is marked Failed and the
        /// run stops (retried later on <see cref="DAInternetChecker.OnInternetRestored"/>).
        /// Sent entries are pruned so the queue cannot grow forever.
        /// </summary>
        private IEnumerator ProcessQueueRoutine()
        {
            _isProcessing = true;

            // Snapshot oldest-first so mutations during the run are safe.
            List<DAPendingEmailData> pending = new List<DAPendingEmailData>(_queue);

            foreach (DAPendingEmailData entry in pending)
            {
                if (entry.status == DAConstants.EMAIL_STATUS_SENT)
                    continue;

                if (DAReportUploader.Instance == null)
                {
                    DALogger.Warn("ProcessQueue: DAReportUploader not found — aborting.");
                    break;
                }

                bool done = false;
                bool ok = false;
                DAReportUploader.Instance.UploadReport(entry, success => { ok = success; done = true; });

                while (!done) yield return null;

                if (ok)
                {
                    entry.status = DAConstants.EMAIL_STATUS_SENT;
                }
                else
                {
                    // Stop on first failure; retried on OnInternetRestored.
                    entry.status = DAConstants.EMAIL_STATUS_FAILED;
                    SaveQueue();
                    _isProcessing = false;
                    yield break;
                }
            }

            // Prune sent entries.
            _queue.RemoveAll(e => e.status == DAConstants.EMAIL_STATUS_SENT);
            SaveQueue();
            _isProcessing = false;
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
