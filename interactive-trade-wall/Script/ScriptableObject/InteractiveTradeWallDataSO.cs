using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[CreateAssetMenu(fileName = "InteractiveTradeWallDataSO",menuName = "Scriptable Objects/InteractiveTradeWallDataSO")]
public class InteractiveTradeWallDataSO:ScriptableObject {
    public TextAsset textAsset;
    public Root root;

    [ContextMenu("Load JSON to ScriptableObject")]
    public void LoadFromJson() {
        if (textAsset == null) {
            Debug.LogError("No Josn File assigned!");
            return;
        }

        root = JsonUtility.FromJson<Root>(textAsset.text);


        Debug.Log("✅ ScriptableObject updated from JSON ");
    }

    [ContextMenu("Save ScriptableObject to JSON")]
    public void SaveToJson() {
        // Convert ScriptableObject to JSON
        string json = JsonUtility.ToJson(root,true);

        // Write file
        File.WriteAllText(textAsset.text,json);

        Debug.Log("JSON saved.");
    }

    [Serializable]
    public class Root {
        public List<Bookmark> bookmarks = new List<Bookmark>();
        public string materialPageBottomLine;
        public string detailPageBottomLine;

        public void Convert(LanguageManager languageManager) {
            if (bookmarks != null) {
                bookmarks.ForEach(b => b.Convert(languageManager));
            }
            Language language = languageManager.CurrentLanguage;
            MarathiTextParser parser = languageManager.GetMarathiTextParser();

            // Translate Bookmark Title
            if (language.Equals(Language.Marathi)) {
                materialPageBottomLine = parser.GetMarathiText(materialPageBottomLine);
                detailPageBottomLine = parser.GetMarathiText(detailPageBottomLine);
            }
        }

        public void CollectAllImagePaths(List<string> paths) {
            if (bookmarks != null) {
                bookmarks.ForEach(b => b.CollectAllImagePaths(paths));
            }
        }
    }


    [Serializable]
    public class Bookmark {
        public string title;
        public int pageNumber;
        public List<BookmarkItem> items = new List<BookmarkItem>();

        public void Convert(LanguageManager languageManager) {
            Language language = languageManager.CurrentLanguage;
            MarathiTextParser parser = languageManager.GetMarathiTextParser();

            // Translate Bookmark Title
            if (language.Equals(Language.Marathi)) {
                title = parser.GetMarathiText(title);
            }

            // Recursively convert items
            if (items != null) {
                items.ForEach(item => item.Convert(languageManager));
            }
        }

        public void CollectAllImagePaths(List<string> paths) {
            if (items != null) {
                items.ForEach(item => item.CollectAllImagePaths(paths));
            }
        }
    }

    [Serializable]
    public class BookmarkItem {
        public string title;
        public string thumbnailPath;
        public string pinnedImagePath;
        public BookmarkMetadata bookmarkMetadata;

        public void Convert(LanguageManager languageManager) {
            Language language = languageManager.CurrentLanguage;
            MarathiTextParser parser = languageManager.GetMarathiTextParser();

            // Translate Item Title
            if (language.Equals(Language.Marathi)) {
                title = parser.GetMarathiText(title);
            }

            // Convert Metadata
            if (bookmarkMetadata != null) {
                bookmarkMetadata.Convert(languageManager);
            }
        }

        public void CollectAllImagePaths(List<string> paths) {
            // 1. Collect Thumbnail
            if (!string.IsNullOrEmpty(thumbnailPath)) {
                paths.Add(thumbnailPath);
            }
            if (!string.IsNullOrEmpty(pinnedImagePath)) {
                paths.Add(pinnedImagePath);
            }
            // 2. Collect Metadata images
            if (bookmarkMetadata != null) {
                bookmarkMetadata.CollectAllImagePaths(paths);
            }
        }
    }

    [Serializable]
    public class BookmarkMetadata {
        public List<BookmarkImage> images = new List<BookmarkImage>();
        public string title;
        public string description;

        public List<Route> importRoutes = new List<Route>();
        public List<Route> exportRoutes = new List<Route>();

        public string distance;
        public string meritimeRoute;
        public string overlandRoute;
        public string challenges;
        public string thenDuration;
        public string nowDuration;
        public string thenMiningProcess;
        public string nowMiningProcess;

        public void Convert(LanguageManager languageManager) {
            Language language = languageManager.CurrentLanguage;
            MarathiTextParser parser = languageManager.GetMarathiTextParser();

            if (language.Equals(Language.Marathi)) {
                title = parser.GetMarathiText(title);
                description = parser.GetMarathiText(description);

                // Translate the extra info fields
                distance = parser.GetMarathiText(distance);
                meritimeRoute = parser.GetMarathiText(meritimeRoute);
                overlandRoute = parser.GetMarathiText(overlandRoute);
                challenges = parser.GetMarathiText(challenges);
                thenDuration = parser.GetMarathiText(thenDuration);
                nowDuration = parser.GetMarathiText(nowDuration);
                thenMiningProcess = parser.GetMarathiText(thenMiningProcess);
                nowMiningProcess = parser.GetMarathiText(nowMiningProcess);
            }

            // Convert Images
            if (images != null) {
                images.ForEach(img => img.Convert(languageManager));
            }

            // Convert Routes (Import)
            if (importRoutes != null) {
                importRoutes.ForEach(route => route.Convert(languageManager));
            }

            // Convert Routes (Export)
            if (exportRoutes != null) {
                exportRoutes.ForEach(route => route.Convert(languageManager));
            }
        }

        public void CollectAllImagePaths(List<string> paths) {
            if (images != null) {
                images.ForEach(img => img.CollectAllImagePaths(paths));
            }

            if (importRoutes != null) {
                importRoutes.ForEach(r => r.CollectAllImagePaths(paths));
            }

            if (exportRoutes != null) {
                exportRoutes.ForEach(r => r.CollectAllImagePaths(paths));
            }
        }
    }

    [Serializable]
    public class Route {
        public List<Hotspot> hotspots = new List<Hotspot>();
        public string routeName;
        public string routeDistance;
        public string routeEra;
        public bool isThisMaritimeRoute;
        public bool isThisOverlandRoute;
        public string challenges;
        public string duration;

        public void Convert(LanguageManager languageManager) {
            Language language = languageManager.CurrentLanguage;
            MarathiTextParser parser = languageManager.GetMarathiTextParser();

            if (language.Equals(Language.Marathi)) {
                routeName = parser.GetMarathiText(routeName);
                challenges = parser.GetMarathiText(challenges);
                duration = parser.GetMarathiText(duration);
                // Translate other fields if necessary
            }

            if (hotspots != null) {
                hotspots.ForEach(h => h.Convert(languageManager));
            }
        }

        public void CollectAllImagePaths(List<string> paths) {
            if (hotspots != null) {
                hotspots.ForEach(h => h.CollectAllImagePaths(paths));
            }
        }
    }

    [Serializable]
    public class Hotspot {
        public string hotspotName;
        public string hotspotThumbnailPath;
        public Position position;

        public void Convert(LanguageManager languageManager) {
            Language language = languageManager.CurrentLanguage;
            MarathiTextParser parser = languageManager.GetMarathiTextParser();

            if (language.Equals(Language.Marathi)) {
                hotspotName = parser.GetMarathiText(hotspotName);
            }
        }

        public void CollectAllImagePaths(List<string> paths) {
            if (!string.IsNullOrEmpty(hotspotThumbnailPath)) {
                paths.Add(hotspotThumbnailPath);
            }
        }
    }

    [Serializable]
    public class Position {
        public float X, Y;
    }

    [Serializable]
    public class BookmarkImage {
        public string imagePath;
        public string title;
        public string subtitle;

        // Added Missing Method
        public void Convert(LanguageManager languageManager) {
            Language language = languageManager.CurrentLanguage;
            MarathiTextParser parser = languageManager.GetMarathiTextParser();

            if (language.Equals(Language.Marathi)) {
                title = parser.GetMarathiText(title);
                subtitle = parser.GetMarathiText(subtitle);
            }
        }

        // Added Missing Method
        public void CollectAllImagePaths(List<string> paths) {
            if (!string.IsNullOrEmpty(imagePath)) {
                paths.Add(imagePath);
            }
        }
    }
}
