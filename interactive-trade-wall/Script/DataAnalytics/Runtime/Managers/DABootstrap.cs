// ============================================================
// DataAnalytics v1.0.0
// DABootstrap.cs
// Zero-configuration auto-installer for all DataAnalytics managers.
// ============================================================

using UnityEngine;
using UnityEngine.EventSystems;
using DataAnalytics.Runtime.Network;
using DataAnalytics.Runtime.Utilities;

namespace DataAnalytics.Runtime.Managers
{
    /// <summary>
    /// Automatically creates every DataAnalytics manager singleton before the first
    /// scene loads, so the host project needs <b>zero</b> manual scene setup — fully
    /// honouring the package's drag-and-drop / minimal-coding philosophy.
    ///
    /// <para>Runs via <see cref="RuntimeInitializeOnLoadMethod"/> in both the Editor
    /// (on entering Play mode) and standalone builds. All created managers are
    /// singletons guarded by <see cref="DontDestroyOnLoad"/>; if the developer also
    /// places any of them manually in a scene, the duplicate destroys itself.</para>
    ///
    /// <para>The only thing the host project must still provide is an
    /// <see cref="EventSystem"/> for UI click detection — this bootstrap warns loudly
    /// if one is missing rather than guessing the correct input module.</para>
    /// </summary>
    public static class DABootstrap
    {
        /// <summary>Name of the root GameObject that hosts all manager components.</summary>
        private const string RootObjectName = "[DA] DataAnalytics";

        private static bool _initialized;

        // ────────────────────────────────────────────────────────────────────────
        // Manager creation (before any scene loads)
        // ────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Creates the persistent root GameObject and attaches all manager singletons.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void CreateManagers()
        {
            if (_initialized) return;
            _initialized = true;

            try
            {
                GameObject root = new GameObject(RootObjectName);
                Object.DontDestroyOnLoad(root);

                // Order matters: the manager owns data + applies settings first,
                // then the supporting services attach.
                root.AddComponent<DAAnalyticsManager>();
                root.AddComponent<DAInternetChecker>();
                root.AddComponent<DAExcelReportGenerator>();
                root.AddComponent<DAReportUploader>();
                root.AddComponent<DAPendingEmailQueue>();
                root.AddComponent<DAAnalyticsScheduler>();
                root.AddComponent<DAIdleTimeTracker>();
            }
            catch (System.Exception ex)
            {
                DALogger.Exception("DABootstrap.CreateManagers", ex);
            }
        }

        // ────────────────────────────────────────────────────────────────────────
        // EventSystem sanity check (after the scene is loaded)
        // ────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Verifies an <see cref="EventSystem"/> exists once the scene has loaded.
        /// Product-click tracking relies on it; a missing one means clicks are
        /// silently dropped, so this warns rather than failing quietly.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void VerifyEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>() == null)
            {
                DALogger.Warn("No EventSystem found in the active scene. " +
                              "DAProductViewCount clicks will NOT be detected until you add one " +
                              "(GameObject > UI > Event System).");
            }
        }
    }
}
