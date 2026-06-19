// ============================================================
// DataAnalytics v1.0.0
// DAProductViewCount.cs
// Tracks product click counts. Attach to any UI element.
// ============================================================

using UnityEngine;
using UnityEngine.EventSystems;
using DataAnalytics.Runtime.Managers;
using DataAnalytics.Runtime.Utilities;

namespace DataAnalytics.Runtime.Components
{
    /// <summary>
    /// Tracks how many times a product UI element is clicked.
    ///
    /// <para><b>Setup:</b> Attach this component directly to any GameObject with a
    /// <see cref="UnityEngine.UI.Button"/>, <see cref="UnityEngine.UI.Image"/>,
    /// or any other UI element. Set <see cref="_productName"/> in the Inspector.
    /// No additional code required.</para>
    ///
    /// <para>Every click increments this product's counter in
    /// <see cref="DAAnalyticsManager"/> automatically.</para>
    /// </summary>
    public class DAProductViewCount : MonoBehaviour, IPointerClickHandler
    {
        // ────────────────────────────────────────────────────────────────────────
        // Inspector
        // ────────────────────────────────────────────────────────────────────────

        [Header("Product Identity")]
        [Space(4)]

        [Tooltip("Unique product name that will appear in analytics reports. " +
                 "Examples: 'Bitumin Product', 'Tile Collection', 'Waterproofing System'. " +
                 "Must be identical across all scenes if the same product appears multiple times.")]
        [SerializeField] private string _productName = "Unnamed Product";

        // ────────────────────────────────────────────────────────────────────────
        // Unity lifecycle
        // ────────────────────────────────────────────────────────────────────────

        private void Start()
        {
            if (string.IsNullOrWhiteSpace(_productName))
            {
                DALogger.Warn($"DAProductViewCount on '{gameObject.name}' has no product name set. " +
                              "Please set it in the Inspector.");
            }

            // Ensure a raycaster exists on the canvas so clicks are received
            EnsureGraphicRaycaster();
        }

        // ────────────────────────────────────────────────────────────────────────
        // IPointerClickHandler
        // ────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Called by the Unity Event System when the GameObject is clicked or tapped.
        /// Increments this product's click counter in <see cref="DAAnalyticsManager"/>.
        /// </summary>
        /// <param name="eventData">Pointer event data (not used directly).</param>
        public void OnPointerClick(PointerEventData eventData)
        {
            if (string.IsNullOrWhiteSpace(_productName)) return;

            if (DAAnalyticsManager.Instance == null)
            {
                DALogger.Warn("DAProductViewCount: DAAnalyticsManager not found in scene.");
                return;
            }

            DAAnalyticsManager.Instance.RecordProductClick(_productName);
        }

        // ────────────────────────────────────────────────────────────────────────
        // Helpers
        // ────────────────────────────────────────────────────────────────────────

        private void EnsureGraphicRaycaster()
        {
            Canvas parentCanvas = GetComponentInParent<Canvas>();
            if (parentCanvas != null &&
                parentCanvas.GetComponent<UnityEngine.UI.GraphicRaycaster>() == null)
            {
                DALogger.Warn($"DAProductViewCount: Canvas '{parentCanvas.gameObject.name}' has no GraphicRaycaster. " +
                              "Clicks may not be detected.");
            }
        }

        // ────────────────────────────────────────────────────────────────────────
        // Public API
        // ────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// The product name used as the analytics key. Can be set in the Inspector
        /// or assigned at runtime. Leave empty/null to skip recording clicks for this
        /// element (e.g. when the active language should not be tracked).
        /// </summary>
        public string ProductName
        {
            get => _productName;
            set => _productName = value;
        }
    }
}
