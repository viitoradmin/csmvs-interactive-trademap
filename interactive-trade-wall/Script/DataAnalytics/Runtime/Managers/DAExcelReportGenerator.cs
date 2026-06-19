// ============================================================
// DataAnalytics v1.0.0
// DAExcelReportGenerator.cs
// Generates a structured CSV weekly analytics report.
// Phase 1: CSV output. Phase 2: swap to ClosedXML .xlsx.
// ============================================================

using System;
using System.IO;
using System.Text;
using UnityEngine;
using DataAnalytics.Runtime.Data;
using DataAnalytics.Runtime.Storage;
using DataAnalytics.Runtime.Utilities;

namespace DataAnalytics.Runtime.Managers
{
    /// <summary>
    /// Generates a structured CSV analytics report for the current tracking week.
    ///
    /// <para><b>Phase 1</b>: Outputs a <c>.csv</c> file containing four labelled sections
    /// (Product, Language, Screen, Idle analytics) openable in Microsoft Excel.</para>
    ///
    /// <para><b>Phase 2</b>: Replace the CSV writer with ClosedXML to produce a proper
    /// <c>.xlsx</c> file with separate worksheets and formatting.</para>
    ///
    /// <para>After successful generation, archives the current JSON and resets the manager.</para>
    /// </summary>
    public class DAExcelReportGenerator : MonoBehaviour
    {
        // ────────────────────────────────────────────────────────────────────────
        // Singleton
        // ────────────────────────────────────────────────────────────────────────

        private static DAExcelReportGenerator _instance;

        /// <summary>Global singleton instance.</summary>
        public static DAExcelReportGenerator Instance => _instance;

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

        // ────────────────────────────────────────────────────────────────────────
        // Public API
        // ────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Generates the weekly CSV report for the supplied analytics data.
        /// On success, archives the JSON and resets analytics for the new week.
        /// </summary>
        /// <param name="data">Analytics data snapshot to report on.</param>
        /// <returns>Absolute path to the generated CSV file, or <c>null</c> on failure.</returns>
        public string GenerateReport(DAAnalyticsData data)
        {
            if (data == null)
            {
                DALogger.Warn("GenerateReport: data is null — skipped.");
                return null;
            }

            try
            {
                string weekLabel  = data.weekStartDate.Replace("-", "_");
                string reportPath = DAStorageManager.BuildReportPath(weekLabel);

                string csv = BuildCsv(data);
                File.WriteAllText(reportPath, csv, Encoding.UTF8);

                DALogger.Log($"{DAConstants.MSG_REPORT_GENERATED} → {reportPath}");

                // Archive the JSON for this week
                DAStorageManager.ArchiveCurrentFile(data.weekStartDate);

                // Reset analytics for new week
                DAAnalyticsManager.Instance.ResetForNewWeek();

                return reportPath;
            }
            catch (Exception ex)
            {
                DALogger.Exception("DAExcelReportGenerator.GenerateReport", ex);
                return null;
            }
        }

        // ────────────────────────────────────────────────────────────────────────
        // CSV builder
        // ────────────────────────────────────────────────────────────────────────

        private string BuildCsv(DAAnalyticsData data)
        {
            var sb = new StringBuilder();

            // ── TITLE ─────────────────────────────────────────────────────────
            sb.AppendLine("INTERACTIVE TRADE WALL  -  ANALYTICS REPORT");
            sb.AppendLine();

            // ── REPORT PERIOD ─────────────────────────────────────────────────
            sb.AppendLine("REPORT PERIOD");
            sb.AppendLine("Field,Value");
            sb.AppendLine($"Week Starting,{FormatDateDDMMYYYY(data.weekStartDate)}");
            sb.AppendLine($"Report Generated,{DateTime.Now.ToString("dd'/'MM'/'yyyy  HH:mm")}");
            sb.AppendLine();

            // ── PRODUCT VIEWS ─────────────────────────────────────────────────
            sb.AppendLine("PRODUCT VIEWS");
            sb.AppendLine("Product Name,View Count");

            if (data.products != null && data.products.Count > 0)
            {
                foreach (var p in data.products)
                    sb.AppendLine($"{EscapeCsv(p.productName.Trim())},{p.clickCount}");
            }
            else
            {
                sb.AppendLine("(no data),0");
            }

            sb.AppendLine();

            // ── LANGUAGE USAGE ────────────────────────────────────────────────
            sb.AppendLine("LANGUAGE USAGE");
            sb.AppendLine("Language,Times Selected,Time Spent (HH:MM)");

            if (data.languages != null && data.languages.Count > 0)
            {
                foreach (var l in data.languages)
                    sb.AppendLine($"{EscapeCsv(l.languageName)},{l.selectionCount},{FormatHHMM(l.totalSeconds)}");
            }
            else
            {
                sb.AppendLine("(no data),0,00:00");
            }

            sb.AppendLine();

            // ── SCREEN VISITS ─────────────────────────────────────────────────
            sb.AppendLine("SCREEN VISITS");
            sb.AppendLine("Screen Name,Visit Count,Time Spent (HH:MM)");

            if (data.screens != null && data.screens.Count > 0)
            {
                foreach (var s in data.screens)
                    sb.AppendLine($"{EscapeCsv(s.screenName)},{s.visitCount},{FormatHHMM(s.totalSeconds)}");
            }
            else
            {
                sb.AppendLine("(no data),0,00:00");
            }

            sb.AppendLine();

            // ── APPLICATION IDLE ──────────────────────────────────────────────
            sb.AppendLine("APPLICATION IDLE");
            sb.AppendLine("Metric,Duration (HH:MM)");
            sb.AppendLine($"Total Idle Time,{FormatHHMM(data.idle?.totalIdleSeconds ?? 0f)}");
            sb.AppendLine();

            // ── NOTES / LEGEND ────────────────────────────────────────────────
            sb.AppendLine("NOTES");
            sb.AppendLine("Column,Description");
            sb.AppendLine("View Count,Number of times a product was tapped / clicked by a visitor during this period");
            sb.AppendLine("Times Selected,Number of times a visitor switched to this language");
            sb.AppendLine("Time Spent (HH:MM),Total time a visitor spent in this language or on this screen (hours:minutes)");
            sb.AppendLine("Total Idle Time,Duration the kiosk was left untouched with no visitor interaction");

            return sb.ToString();
        }

        // ────────────────────────────────────────────────────────────────────────
        // Helpers
        // ────────────────────────────────────────────────────────────────────────

        /// <summary>Converts "yyyy-MM-dd" to "DD/MM/YYYY" without culture dependency.</summary>
        private static string FormatDateDDMMYYYY(string isoDate)
        {
            if (string.IsNullOrEmpty(isoDate)) return isoDate;
            string[] parts = isoDate.Split('-');
            if (parts.Length == 3) return $"{parts[2]}/{parts[1]}/{parts[0]}";
            return isoDate;
        }

        /// <summary>Converts total seconds to "HH:MM".</summary>
        private static string FormatHHMM(float totalSeconds)
        {
            int t = (int)totalSeconds;
            return $"{t / 3600:D2}:{(t % 3600) / 60:D2}";
        }

        /// <summary>
        /// Escapes a string value for safe inclusion inside a CSV field.
        /// Wraps in double quotes if it contains commas, quotes, or newlines.
        /// </summary>
        private static string EscapeCsv(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
                return $"\"{value.Replace("\"", "\"\"")}\"";
            return value;
        }
    }
}
