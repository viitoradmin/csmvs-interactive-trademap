// ============================================================
// DataAnalytics v1.0.0
// DAProductAnalytics.cs
// Tracks how many times a specific product has been clicked.
// ============================================================

using System;

namespace DataAnalytics.Runtime.Data
{
    /// <summary>
    /// Stores the click count for a single named product.
    /// One entry per unique <see cref="productName"/> in the analytics list.
    /// </summary>
    [Serializable]
    public class DAProductAnalytics
    {
        /// <summary>
        /// Unique display name of the product as shown in analytics reports.
        /// Must exactly match the <c>productName</c> field set on the
        /// <c>DAProductViewCount</c> component.
        /// </summary>
        public string productName = string.Empty;

        /// <summary>
        /// Total number of times this product's UI element was clicked
        /// during the current tracking week.
        /// </summary>
        public int clickCount = 0;
    }
}
