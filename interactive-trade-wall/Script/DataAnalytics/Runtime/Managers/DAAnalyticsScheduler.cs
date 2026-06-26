// ============================================================
// DataAnalytics v1.0.0
// DAAnalyticsScheduler.cs
// Watches the clock and triggers report generation at the configured time.
// ============================================================

using System.Collections;
using UnityEngine;
using DataAnalytics.Runtime.Data;
using DataAnalytics.Runtime.Storage;
using DataAnalytics.Runtime.Utilities;

namespace DataAnalytics.Runtime.Managers
{
    /// <summary>
    /// Singleton MonoBehaviour that runs a continuous clock-check loop.
    /// When the configured report time arrives (default: Saturday 20:00 IST),
    /// it triggers <see cref="DAExcelReportGenerator"/> to produce the weekly CSV,
    /// then routes the report to the email queue or pending queue depending on
    /// internet availability.
    ///
    /// <para>Survives scene loads via <see cref="DontDestroyOnLoad"/>.</para>
    /// </summary>
    public class DAAnalyticsScheduler : MonoBehaviour
    {
        // ────────────────────────────────────────────────────────────────────────
        // Singleton
        // ────────────────────────────────────────────────────────────────────────

        private static DAAnalyticsScheduler _instance;

        /// <summary>Global singleton instance.</summary>
        public static DAAnalyticsScheduler Instance => _instance;

        // ────────────────────────────────────────────────────────────────────────
        // Constants
        // ────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// How often (seconds) the scheduler checks the clock. Kept well below 60s
        /// so the one-minute report window is never missed due to coroutine drift.
        /// </summary>
        private const float POLL_INTERVAL_SECONDS = 20f;

        // ────────────────────────────────────────────────────────────────────────
        // Runtime state
        // ────────────────────────────────────────────────────────────────────────

        private bool      _reportGeneratedThisMinute;
        private Coroutine _schedulerCoroutine;

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
            _schedulerCoroutine = StartCoroutine(SchedulerLoop());
        }

        private void OnDestroy()
        {
            if (_schedulerCoroutine != null)
                StopCoroutine(_schedulerCoroutine);
        }

        // ────────────────────────────────────────────────────────────────────────
        // Scheduler loop
        // ────────────────────────────────────────────────────────────────────────

        private IEnumerator SchedulerLoop()
        {
            while (true)
            {
                // Poll well within the one-minute target window so clock drift can
                // never cause the scheduler to skip the exact report minute.
                yield return new WaitForSecondsRealtime(POLL_INTERVAL_SECONDS);

                bool isReportTime = DATimeUtility.IsReportTime();

                if (isReportTime && !_reportGeneratedThisMinute)
                {
                    _reportGeneratedThisMinute = true;
                    TriggerReportGeneration();
                }
                else if (!isReportTime)
                {
                    // Reset the guard so it can trigger again next week
                    _reportGeneratedThisMinute = false;
                }
            }
        }

        // ────────────────────────────────────────────────────────────────────────
        // Report generation + routing
        // ────────────────────────────────────────────────────────────────────────

        private void TriggerReportGeneration()
        {
            DAAnalyticsData data = DAAnalyticsManager.Instance?.Data;
            if (data == null)
            {
                DALogger.Error("Scheduler: DAAnalyticsManager.Data is null — cannot generate report.");
                return;
            }

            // Generate CSV report (persists in Reports/).
            string reportPath = DAExcelReportGenerator.Instance?.GenerateReport(data);

            if (string.IsNullOrEmpty(reportPath))
            {
                DALogger.Error("Scheduler: Report generation failed — not queuing for upload.");
                return;
            }

            // Enqueue so a failed upload can retry from the persisted report.
            DAPendingEmailData entry = DAPendingEmailData.CreateNew(data.weekStartDate, reportPath);
            DAPendingEmailQueue.Instance?.EnqueueReport(entry);

            // Upload now if online; otherwise it stays queued and retries on
            // OnInternetRestored.
            bool hasInternet = DAInternetChecker.Instance != null
                               && DAInternetChecker.Instance.IsConnected;

            if (hasInternet)
                DAPendingEmailQueue.Instance?.ProcessQueue();
        }

        // ────────────────────────────────────────────────────────────────────────
        // Public API
        // ────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Manually triggers report generation outside of the normal schedule.
        /// Useful for testing or manual admin actions.
        /// </summary>
        [ContextMenu("Force Generate Report Now")]
        public void ForceGenerateNow()
        {
            TriggerReportGeneration();
        }
    }
}
