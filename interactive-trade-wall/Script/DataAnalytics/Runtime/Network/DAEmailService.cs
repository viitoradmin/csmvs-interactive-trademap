// ============================================================
// DataAnalytics v1.0.0
// DAEmailService.cs
// PHASE 2 STUB — email send logic not yet implemented.
// ============================================================

using System.Threading.Tasks;
using DataAnalytics.Runtime.Data;
using DataAnalytics.Runtime.Utilities;

namespace DataAnalytics.Runtime.Network
{
    /// <summary>
    /// <b>PHASE 2 STUB</b> — Email delivery service for the DataAnalytics package.
    ///
    /// <para>This class is intentionally empty in Phase 1. All method signatures
    /// are defined so that <see cref="DAPendingEmailQueue"/> and
    /// <see cref="DAAnalyticsScheduler"/> can reference them without changes
    /// when Phase 2 is implemented.</para>
    ///
    /// <para><b>Phase 2 implementation plan:</b></para>
    /// <list type="bullet">
    ///   <item>Add MailKit and MimeKit DLLs to <c>Assets/DataAnalytics/Plugins/</c></item>
    ///   <item>Reference them in <c>DataAnalytics.Runtime.asmdef</c></item>
    ///   <item>Implement <see cref="SendReportAsync"/> using Office365 SMTP</item>
    ///   <item>Configure SMTP credentials in <see cref="DASettings"/> (encrypted)</item>
    ///   <item>All email is sent directly from Unity — no browser, no external executable</item>
    /// </list>
    ///
    /// <para><b>Preferred Phase 2 stack:</b></para>
    /// <code>
    /// MailKit + MimeKit + Office365 SMTP (smtp.office365.com:587, STARTTLS)
    /// </code>
    /// </summary>
    public static class DAEmailService
    {
        // ────────────────────────────────────────────────────────────────────────
        // Phase 2 stubs
        // ────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// <b>PHASE 2</b> — Sends the weekly analytics CSV report to all configured
        /// recipients via Office365 SMTP using MailKit.
        ///
        /// <para>Currently logs a Phase 2 stub message and returns <c>false</c>.</para>
        /// </summary>
        /// <param name="entry">The pending email entry containing the report path and metadata.</param>
        /// <returns>
        /// A <see cref="Task{bool}"/> that resolves to <c>true</c> on successful delivery,
        /// <c>false</c> on failure. Always returns <c>false</c> in Phase 1.
        /// </returns>
        public static Task<bool> SendReportAsync(DAPendingEmailData entry)
        {
            // PHASE 2 IMPLEMENTATION TEMPLATE:
            //
            // var message = new MimeMessage();
            // message.From.Add(new MailboxAddress("Kiosk Analytics", settings.SenderEmail));
            // foreach (var recipient in settings.EmailRecipients)
            //     message.To.Add(new MailboxAddress("", recipient));
            // message.Subject = $"Weekly Analytics Report — {entry.reportWeek}";
            //
            // var builder = new BodyBuilder();
            // builder.TextBody = "Please find the attached weekly analytics report.";
            // builder.Attachments.Add(entry.excelPath);
            // message.Body = builder.ToMessageBody();
            //
            // using var client = new SmtpClient();
            // await client.ConnectAsync("smtp.office365.com", 587, SecureSocketOptions.StartTls);
            // await client.AuthenticateAsync(settings.SenderEmail, settings.SenderPassword);
            // await client.SendAsync(message);
            // await client.DisconnectAsync(true);
            //
            // return true;

            return Task.FromResult(false);
        }

        /// <summary>
        /// <b>PHASE 2</b> — Tests SMTP connectivity without sending a real email.
        /// Currently always returns <c>false</c>.
        /// </summary>
        public static Task<bool> TestConnectionAsync()
        {
            return Task.FromResult(false);
        }
    }
}
