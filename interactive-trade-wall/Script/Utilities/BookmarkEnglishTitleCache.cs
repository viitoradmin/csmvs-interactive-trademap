using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static InteractiveTradeWallDataSO;

/// <summary>
/// Lazy-loaded lookup: uuid → English product title.
/// Built once from the cached TradeMapData.json (English).
/// Used by ItemElement so analytics always records the English name,
/// even when the active language is Marathi.
/// </summary>
public static class BookmarkEnglishTitleCache
{
    private static Dictionary<string, string> _cache;

    /// <summary>
    /// Returns the English title for the given product UUID.
    /// Falls back to the uuid string itself if the cache file is missing
    /// or the uuid is not found.
    /// </summary>
    public static string GetEnglishTitle(string uuid)
    {
        if (string.IsNullOrEmpty(uuid)) return uuid;

        if (_cache == null)
            BuildCache();

        return (_cache != null && _cache.TryGetValue(uuid, out string title)) ? title : uuid;
    }

    /// <summary>
    /// Clears the in-memory cache so it is rebuilt on the next access.
    /// Call this if the English data file is refreshed at runtime.
    /// </summary>
    public static void Invalidate() => _cache = null;

    private static void BuildCache()
    {
        string path = Path.Combine(Application.persistentDataPath, "TradeMapData.json");

        if (!File.Exists(path))
        {
            Debug.LogWarning("[BookmarkEnglishTitleCache] TradeMapData.json not found. " +
                             "Analytics will fall back to product UUIDs until English data is cached.");
            _cache = new Dictionary<string, string>();
            return;
        }

        try
        {
            string json = File.ReadAllText(path);
            Root root = JsonUtility.FromJson<Root>(json);

            _cache = new Dictionary<string, string>();

            if (root?.bookmarks == null) return;

            foreach (Bookmark bookmark in root.bookmarks)
            {
                if (bookmark?.items == null) continue;
                foreach (BookmarkItem item in bookmark.items)
                {
                    if (!string.IsNullOrEmpty(item.uuid) && !string.IsNullOrEmpty(item.title))
                        _cache[item.uuid] = item.title;
                }
            }

            Debug.Log($"[BookmarkEnglishTitleCache] Loaded {_cache.Count} uuid→title entries.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[BookmarkEnglishTitleCache] Failed to build cache: {ex.Message}");
            _cache = new Dictionary<string, string>();
        }
    }
}
