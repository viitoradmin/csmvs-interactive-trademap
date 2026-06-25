// ============================================================
// DataAnalytics v1.0.0
// DAReportUploader.cs
// Uploads the weekly report to the backend (multipart POST).
// Backend emails it — no email credentials live in the build.
// ============================================================

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using DataAnalytics.Runtime.Data;
using DataAnalytics.Runtime.Utilities;

namespace DataAnalytics.Runtime.Network
{
    /// <summary>
    /// Uploads a generated report file to the backend via multipart/form-data.
    /// The backend authenticates with an API key, attaches CSV + XLSX, and emails
    /// the configured recipients. Replaces the old direct-SMTP plan so no email
    /// credentials ever ship inside the Unity build.
    ///
    /// <para>Survives scene loads via <see cref="DontDestroyOnLoad"/>.</para>
    /// </summary>
    public class DAReportUploader : MonoBehaviour
    {
        // ────────────────────────────────────────────────────────────────────────
        // Singleton
        // ────────────────────────────────────────────────────────────────────────

        private static DAReportUploader _instance;

        /// <summary>Global singleton instance.</summary>
        public static DAReportUploader Instance => _instance;

        private const int UPLOAD_TIMEOUT_SECONDS = 60;

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

        // ────────────────────────────────────────────────────────────────────────
        // Public API
        // ────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Uploads the report referenced by <paramref name="entry"/> to the backend.
        /// Invokes <paramref name="onComplete"/> with <c>true</c> only on HTTP 2xx.
        /// </summary>
        public void UploadReport(DAPendingEmailData entry, Action<bool> onComplete)
        {
            StartCoroutine(UploadRoutine(entry, onComplete));
        }

        // ────────────────────────────────────────────────────────────────────────
        // Upload coroutine
        // ────────────────────────────────────────────────────────────────────────

        private IEnumerator UploadRoutine(DAPendingEmailData entry, Action<bool> onComplete)
        {
            DASettings settings = DASettingsLoader.Settings;

            // Fail closed when not configured.
            if (settings == null ||
                string.IsNullOrWhiteSpace(settings.BackendUploadUrl) ||
                string.IsNullOrWhiteSpace(settings.UploadApiKey))
            {
                DALogger.Warn(DAConstants.MSG_UPLOAD_SKIPPED);
                onComplete?.Invoke(false);
                yield break;
            }

            if (entry == null || string.IsNullOrWhiteSpace(entry.excelPath) || !File.Exists(entry.excelPath))
            {
                DALogger.Warn($"{DAConstants.MSG_UPLOAD_FAILED} Report file missing: {entry?.excelPath}");
                onComplete?.Invoke(false);
                yield break;
            }

            byte[] fileBytes;
            try
            {
                fileBytes = File.ReadAllBytes(entry.excelPath);
            }
            catch (Exception ex)
            {
                DALogger.Exception("DAReportUploader.ReadFile", ex);
                onComplete?.Invoke(false);
                yield break;
            }

            string fileName = Path.GetFileName(entry.excelPath);

            var form = new List<IMultipartFormSection>
            {
                new MultipartFormFileSection(
                    DAConstants.UPLOAD_FIELD_FILE, fileBytes, fileName, DAConstants.UPLOAD_CSV_MIME),
                new MultipartFormDataSection(DAConstants.UPLOAD_FIELD_WEEK, entry.reportWeek),
                new MultipartFormDataSection(DAConstants.UPLOAD_FIELD_DEVICE, ResolveDeviceId(settings)),
                new MultipartFormDataSection(DAConstants.UPLOAD_FIELD_APP_NAME, ResolveAppName(settings)),
            };

            using (UnityWebRequest req = UnityWebRequest.Post(settings.BackendUploadUrl, form))
            {
                req.SetRequestHeader(DAConstants.UPLOAD_HEADER_API_KEY, settings.UploadApiKey);
                req.timeout = UPLOAD_TIMEOUT_SECONDS;

                yield return req.SendWebRequest();

                bool success = req.result == UnityWebRequest.Result.Success
                               && req.responseCode >= 200 && req.responseCode < 300;

                if (!success)
                {
                    DALogger.Warn($"{DAConstants.MSG_UPLOAD_FAILED} " +
                                  $"HTTP {req.responseCode} {req.error} — week {entry.reportWeek}");
                }

                onComplete?.Invoke(success);
            }
        }

        // ────────────────────────────────────────────────────────────────────────
        // Helpers
        // ────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Resolves the device id sent to the backend. Uses <see cref="DASettings.DeviceId"/>
        /// when set, else <see cref="SystemInfo.deviceUniqueIdentifier"/>.
        /// When <see cref="DASettings.BypassDuplicateCheck"/> is on, appends a unique
        /// suffix so the backend never dedupes (TEMP testing only).
        /// </summary>
        private static string ResolveDeviceId(DASettings settings)
        {
            string id = !string.IsNullOrWhiteSpace(settings.DeviceId)
                ? settings.DeviceId
                : SystemInfo.deviceUniqueIdentifier;

            if (settings.BypassDuplicateCheck)
                id = $"{id}-test-{DateTime.Now.Ticks}";

            return id;
        }

        private static string ResolveAppName(DASettings settings)
        {
            return !string.IsNullOrWhiteSpace(settings.AppDisplayName)
                ? settings.AppDisplayName
                : Application.productName;
        }
    }
}
