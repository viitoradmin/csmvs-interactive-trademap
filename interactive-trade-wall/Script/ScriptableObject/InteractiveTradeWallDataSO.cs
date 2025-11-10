using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[CreateAssetMenu(fileName = "InteractiveTradeWallDataSO", menuName = "Scriptable Objects/InteractiveTradeWallDataSO")]
public class InteractiveTradeWallDataSO : ScriptableObject
{
    public TextAsset textAsset;
    public Root root;

    [ContextMenu("Load JSON to ScriptableObject")]
    public void LoadFromJson()
    {
        if (textAsset == null)
        {
            Debug.LogError("No Josn File assigned!");
            return;
        }
        
        root = JsonUtility.FromJson<Root>(textAsset.text);


        Debug.Log("✅ ScriptableObject updated from JSON ");
    }

    [ContextMenu("Save ScriptableObject to JSON")]
    public void SaveToJson()
    {
        // Convert ScriptableObject to JSON
        string json = JsonUtility.ToJson(root, true);

        // Write file
        File.WriteAllText(textAsset.text, json);

        Debug.Log("JSON saved.");
    }
    [Serializable]
    public class Root
    {
        public List<Bookmark> bookmarks = new List<Bookmark>();
        public string materialPageBottomLine;
        public string detailPageBottomLine;
    }


    [Serializable]
    public class Bookmark
    {
        public string title;
        public string title_marathi;
        public int pageNumber;
        public List<BookmarkItem> items = new List<BookmarkItem>();
    }

    [Serializable]
    public class BookmarkItem
    {
        public string title;
        public string title_marathi;
        public string thumbnailPath;
        public BookmarkMetadata bookmarkMetadata;
    }

    [Serializable]
    public class BookmarkMetadata
    {
        public List<BookmarkImage> images = new List<BookmarkImage>();
        public string title;
        public string title_marathi;
        public string description;
        public string description_marathi;
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
    }

    [Serializable]
    public class Route
    {
        public List<Hotspot> hotspots = new List<Hotspot>();
        public string routeName;
        public string routeDistance;
        public string routeEra;
        public bool isThisMaritimeRoute;
        public bool isThisOverlandRoute;
        public string challenges;
        public string duration;
    }

    [Serializable]
    public class Hotspot
    {
        public string hotspotName;
        public string hotspotThumbnailPath;
        public Position position;
    }

    [Serializable]
    public class Position
    {
        public float X, Y;
    }

    [Serializable]
    public class BookmarkImage
    {
        public string imagePath;
        public string title;
        public string subtitle;
    }
}
