# DataAnalytics v1.0.0
### Reusable Offline Analytics Package for Unity Kiosk Applications

Auto-tracking analytics for Unity Windows touch-screen kiosk applications.
Records product views, language usage, screen visits, and idle time.
Generates a weekly CSV report and (Phase 2) emails it automatically.

---

## Table of Contents

- [Quick Start](#quick-start)
- [Storage Folder Structure](#storage-folder-structure)
- [Managers Reference](#managers-reference)
- [Components Reference](#components-reference)
- [Weekly Report Flow](#weekly-report-flow)
- [Saturday Close → Sunday Open Scenario](#saturday-close--sunday-open-scenario)
- [Industry-Standard Report Delivery](#industry-standard-report-delivery)
- [Auto-Save Behaviour](#auto-save-behaviour)
- [Configuration](#configuration)
- [Namespace Reference](#namespace-reference)
- [Logging](#logging)
- [Portability](#portability)
- [Phase 2 Roadmap](#phase-2-roadmap)
- [Known Limitations](#known-limitations)

---

## Quick Start

### Managers — Nothing to do (Auto-Bootstrap)

All managers are created **automatically** before the first scene loads by
`DABootstrap` (via `[RuntimeInitializeOnLoadMethod]`). You do **not** need to add
any GameObject or component to your scenes. A persistent root object named
`[DA] DataAnalytics` is created at runtime and survives all scene changes via
`DontDestroyOnLoad`.

Managers created automatically:

```
DAAnalyticsManager
DAInternetChecker
DAExcelReportGenerator
DAPendingEmailQueue
DAAnalyticsScheduler
DAIdleTimeTracker
```

> **Note:** Your scene must contain an `EventSystem` (GameObject → UI → Event System)
> for product-click tracking to work. The bootstrap logs a warning if one is missing.

### Step 1 — Attach Components to GameObjects

| Component | Attach To | What it tracks |
|---|---|---|
| `DAProductViewCount` | Any clickable product UI element | Click / tap count per product |
| `DALanguageTracker` | `LanguageButtonHandler` GameObject | Language selected + time spent |
| `DAScreenTracker` | Any screen root GameObject | Screen visits + time spent |

> `DALanguageTracker` is **not** a singleton — attach one instance per GameObject.
> Call `SetActiveLanguage()` from code; do not configure via Inspector toggle.

### Step 2 — Configure Settings

Open `Resources/DASettings.asset` in the Unity Inspector.
All configuration is done here — no code changes required.

---

## Storage Folder Structure

All files live inside `Application.persistentDataPath/DataAnalytics/`.

On Windows (this project):
```
C:\Users\<user>\AppData\LocalLow\ViitorCloud\Interactive Trade Wall\DataAnalytics\
```

```
DataAnalytics/
├── analytics_current.json    ← Live data being recorded right now
│
├── Archive/                  ← Old weekly JSON files (raw data, never deleted)
│   ├── analytics_2026-06-15.json
│   └── analytics_2026-06-08.json
│
├── Reports/                  ← Generated CSV reports, one per week
│   └── WeeklyReport_2026_06_15.csv
│
├── PendingReports/           ← Reports waiting to be emailed (no internet at report time)
│   └── WeeklyReport_2026_06_08.csv
│
└── Queue/
    └── email_queue.json      ← Tracks which reports have been sent / are still pending
```

### analytics_current.json

**What it is:** The live analytics file. Every product click, language switch,
screen visit, and idle period is recorded here in memory and flushed to this file.

**When written:** Every `SaveIntervalSeconds` (default 30s), on `ApplicationPause`,
`ApplicationFocus(false)`, and `ApplicationQuit`.

**When replaced:** At the start of every new week (Monday). The old file is moved
to `Archive/` before a fresh one is created.

### Archive/

**What it is:** Permanent record of every past week's raw JSON data.

**When files arrive:** On new week detection (Monday launch) and after a successful
report generation. Files are **never overwritten** — a counter suffix is appended
if a file already exists (`analytics_2026-06-15_1.json`).

> Never delete this folder manually. It is your source of truth for all raw data.

### Reports/

**What it is:** The clean output folder. Contains the final `.csv` report files
openable in Excel.

**When files arrive:** Every Saturday at the configured report time (default 20:00 IST),
provided the app is running at that moment.

**File naming:** `WeeklyReport_2026_06_15.csv` (week-start Monday date).

**CSV structure:**
```
INTERACTIVE TRADE WALL  -  ANALYTICS REPORT

REPORT PERIOD
PRODUCT VIEWS       ← English product names, view counts
LANGUAGE USAGE      ← Times selected + total time per language (HH:MM)
SCREEN VISITS       ← Visit count + total time per screen (HH:MM)
APPLICATION IDLE    ← Total time kiosk was untouched
NOTES               ← Column descriptions / legend
```

### PendingReports/

**What it is:** Holding folder for reports that were generated but could not be
emailed due to no internet at report time.

**What happens next:** When internet is restored, `DAInternetChecker` fires
`OnInternetRestored` → `DAPendingEmailQueue` retries all files here, oldest first.

### Queue/ — email_queue.json

**What it is:** A JSON list of every report queued for email, with a status per entry:
`Pending`, `Sent`, or `Failed`. Survives app restarts — prevents double-sends and
silent data loss.

```json
{
  "queue": [
    {
      "reportWeek": "2026-06-15",
      "excelPath": "...PendingReports/WeeklyReport_2026_06_15.csv",
      "createdAt": "2026-06-20 20:00:00",
      "status": "Pending"
    }
  ]
}
```

---

## Managers Reference

| Manager | Purpose |
|---|---|
| `DAAnalyticsManager` | Holds all analytics data in memory, auto-saves every 30s |
| `DAAnalyticsScheduler` | Checks clock every 20s, triggers weekly report generation |
| `DAExcelReportGenerator` | Writes the weekly CSV to `Reports/` folder |
| `DAInternetChecker` | Polls connectivity, fires `OnInternetRestored` event |
| `DAPendingEmailQueue` | Queues and retries reports when internet returns |
| `DAIdleTimeTracker` | Detects idle periods from touch / mouse / keyboard input |

---

## Components Reference

### DAProductViewCount

Attach to any product UI element. Implements `IPointerClickHandler`.

```csharp
// Set at runtime by ItemElement.SetupData()
// UUID deduplicates across EN/MR; BookmarkEnglishTitleCache resolves uuid → English title
productViewCount.ProductName = BookmarkEnglishTitleCache.GetEnglishTitle(m_ItemData.uuid);
```

- Leave `ProductName` empty/null to skip tracking for that element.
- If `DAAnalyticsManager` is not found, the click is silently skipped (no exception).

### DALanguageTracker

Attach to the `LanguageButtonHandler` GameObject. Drive from code only.

```csharp
daLanguageTracker.SetActiveLanguage("English");  // or "Marathi"
```

- Tracks selection count and cumulative time per language.
- Commits duration on `OnDisable`, `OnApplicationPause`, `OnApplicationFocus(false)`,
  and `OnApplicationQuit`.
- De-duplicates: calling `SetActiveLanguage` with the same language twice is a no-op.

### DAScreenTracker

Attach to any screen root. Set `Screen Name` in the Inspector.

- Starts timing when the GameObject becomes active (`OnEnable`).
- Commits visit count + duration when the GameObject is deactivated (`OnDisable`).
- Works with `GameObject.SetActive(true/false)` — no extra code required.

---

## Weekly Report Flow

```
MONDAY 00:00
  └─ App detects new week on launch
       ├─ Archives analytics_current.json → Archive/analytics_<week>.json
       └─ Creates fresh analytics_current.json

MON – SAT (all week)
  └─ Visitors use the kiosk
       ├─ DAProductViewCount  → records product taps
       ├─ DALanguageTracker   → records language switches + duration
       ├─ DAScreenTracker     → records screen visits + duration
       └─ DAIdleTimeTracker   → records idle periods
       (autosaved to analytics_current.json every 30s)

SATURDAY 20:00 IST  ← DAAnalyticsScheduler fires
  └─ DAExcelReportGenerator.GenerateReport()
       ├─ Builds WeeklyReport_<week>.csv → Reports/
       ├─ Archives analytics_current.json → Archive/
       ├─ Resets analytics for new week
       └─ Internet check:
            ├─ Online  → queue for immediate email send (Phase 2)
            └─ Offline → copy to PendingReports/ + add to email_queue.json
                          └─ On internet restore → DAPendingEmailQueue retries
```

---

## Saturday Close → Sunday Open Scenario

**Situation:** Museum closes at 17:30 on Saturday. App shuts down.
The configured report time is **Saturday 20:00 IST** — 2.5 hours after closing.

### What actually happens

```
Saturday 17:30 — App closes
  └─ OnApplicationQuit() → SaveNow()
       └─ analytics_current.json saved with full week's data  ✓

Saturday 20:00 — App is NOT running
  └─ DAAnalyticsScheduler cannot fire
       └─ NO report generated, NO email sent  ✗

Sunday morning — App opens
  └─ DAAnalyticsManager loads analytics_current.json
       ├─ weekStartDate = "2026-06-16" (Monday)
       └─ current week  = "2026-06-16" (Sunday is still the same Mon–Sun week)
            └─ SAME WEEK → data resumed, nothing archived  ✓

  └─ DAPendingEmailQueue loads email_queue.json → EMPTY
       └─ Nothing to send (report was never generated)

  └─ DAAnalyticsScheduler starts
       └─ IsReportTime() → Sunday ≠ Saturday 20:00 → false
            └─ Waits until next Saturday 20:00
```

### Outcome

| | Result |
|---|---|
| Week's raw data | **Safe** — saved on close |
| CSV report | **Not generated** — scheduler missed the window |
| Email | **Not sent** |
| Next Monday | Data archived as JSON only — no CSV report ever produced |

### Current workaround — Force Generate

1. Open Unity, enter Play mode
2. In the Hierarchy, find `[DA] AnalyticsScheduler`
3. Right-click `DAAnalyticsScheduler` component → **"Force Generate Report Now"**
4. CSV is created in `Reports/` and queued for email

### Recommended fix (not yet implemented)

Persist a `reportGeneratedThisWeek: bool` flag inside `analytics_current.json`.
On app launch, if it is past Saturday 20:00 and the flag is `false`, generate
the report immediately before starting the new week.

---

## Industry-Standard Report Delivery

The current system schedules on the device. Every major analytics platform
(Google Analytics, Mixpanel, Amplitude) solves this differently:

```
CLIENT (Unity kiosk)
  ├─ Records events locally
  ├─ Sends batches to cloud API every 5 minutes
  └─ On app quit → flush remaining events

CLOUD (always running — does not depend on kiosk state)
  ├─ Receives events → stores in database
  ├─ CRON JOB: every Saturday 20:00
  │     → query this week's data
  │     → generate report
  │     └─ send via transactional email service
  └─ Retry logic built into cloud infrastructure

EMAIL SERVICE (SendGrid / AWS SES / Mailgun)
  ├─ Handles delivery retries automatically
  ├─ Tracks delivery / bounce / open
  └─ Guaranteed delivery or failure notification
```

### Recommended upgrade path for this project

Since the project already has a backend (login + API):

| Option | Approach | Cost |
|---|---|---|
| **Extend existing backend** *(recommended)* | Add `POST /analytics/sync` + cron job + SendGrid | Zero (existing infra) |
| Firebase | Unity Firebase SDK → Firestore → Scheduled Functions → SendGrid | Near zero at this scale |
| n8n (no-code) | Kiosk webhook → n8n cron → email | Free self-hosted |

**Immediate fix without a server:** Change `ReportHour` to `17` in `DASettings.asset`
so the report fires at 17:00 IST — before the museum closes at 17:30.

---

## Auto-Save Behaviour

Data is saved to `analytics_current.json`:

- Every `SaveIntervalSeconds` seconds (default: 30s, configurable)
- On `OnApplicationPause(true)`
- On `OnApplicationFocus(false)`
- On `OnApplicationQuit()`

**Zero data loss** — the system never relies on a clean shutdown.

---

## Configuration

Edit `Resources/DASettings.asset` in the Unity Inspector.

| Setting | Default | Description |
|---|---|---|
| `SaveIntervalSeconds` | 30 | How often analytics_current.json is autosaved |
| `ReportDay` | Saturday | Day of week to generate the weekly report |
| `ReportHour` | 20 | Hour (24h, IST) to generate the report |
| `ReportMinute` | 0 | Minute to generate the report |
| `Timezone` | India Standard Time | Windows timezone ID for all scheduling |
| `EnableHttpPing` | true | Whether to ping a URL to check internet |
| `HttpPingUrl` | https://clients3.google.com/generate_204 | URL used for internet check (must be HTTPS) |
| `EnableLogging` | true | Toggle `[DataAnalytics]` console logs |

---

## Namespace Reference

```csharp
using DataAnalytics.Runtime;                  // DASettings
using DataAnalytics.Runtime.Components;       // DAProductViewCount, DALanguageTracker, DAScreenTracker
using DataAnalytics.Runtime.Managers;         // DAAnalyticsManager, DAAnalyticsScheduler, etc.
using DataAnalytics.Runtime.Data;             // DAAnalyticsData, DAProductAnalytics, etc.
using DataAnalytics.Runtime.Storage;          // DAStorageManager
using DataAnalytics.Runtime.Network;          // DAEmailService (Phase 2)
using DataAnalytics.Runtime.Utilities;        // DAConstants, DALogger, DATimeUtility
```

---

## Logging

All logs are prefixed: `[DataAnalytics]`

Toggle via `DASettings.asset → Enable Logging`.
Errors are always logged regardless of the toggle setting.

---

## Portability

To use this package in another project:

1. Copy the entire `DataAnalytics/` folder into `Assets/`
2. Ensure `DASettings.asset` exists in `Resources/` (create via
   **Assets → Create → DataAnalytics → Settings** if missing)
3. Add an `EventSystem` to your scene if one does not already exist
4. Done — managers auto-bootstrap at runtime; no other setup required

---

## Phase 2 Roadmap

- [ ] Implement `DAEmailService.SendReportAsync()` with SMTP / SendGrid API
- [ ] Add sender credentials to `DASettings.asset`
- [ ] Add missed-report recovery on app launch (`reportGeneratedThisWeek` flag)
- [ ] Auto-clean `PendingReports/` files after confirmed email delivery
- [ ] Add ClosedXML for proper `.xlsx` output with formatted worksheets
- [ ] Move scheduler to server-side cron for guaranteed delivery

---

## Known Limitations

| # | Issue | Severity | Status |
|---|---|---|---|
| 1 | Report not generated if app is closed before Saturday 20:00 | Medium | Manual workaround: Force Generate or change ReportHour to 17 |
| 2 | Email sending not implemented | Medium | Phase 2 planned |
| 3 | `DAInternetChecker` HTTP ping causes `InsecureConnection` error in Unity 6 | Low | Use `https://` URL in DASettings |
| 4 | `PendingReports/` files not auto-deleted after email delivery | Low | Phase 2 cleanup |

---

*DataAnalytics v1.0.0 — Built for ViitorCloud Unity Kiosk Projects*
