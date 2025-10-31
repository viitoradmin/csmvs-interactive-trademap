using System;
using System.Collections.Generic;

public class InteractiveTradeWallStaticClass{
    [Serializable]
    public class Data {
        public List<Bookmark> bookmarks = new List<Bookmark>();
    }

    //public class RootData {//Only Tab app
    //    public List<Route> ImportedRoots = new List<Route>();
    //    public List<Route> ExportedRoots = new List<Route>();
    //}
    [Serializable]
    public class Bookmark {
        public string title;
        public int pageNumber;
        public List<BookmarkItem> items = new List<BookmarkItem>();
    }
    [Serializable]
    public class BookmarkItem {
        public string title;
        public string thumbnailPath;
        public BookmarkMetadata bookmarkMetadata;//Only Multiple app
        //public List<Hotspot> Hotspots = new List<Hotspot>(); //Only Tab app
        //public List<BookmarkImage> HotspotImages = new List<BookmarkImage>(); //Only Tab app
    }
    [Serializable]
    public class BookmarkMetadata {
        public List<BookmarkImage> images = new List<BookmarkImage>();
        public string title;
        public string description;
        public List<Route> importRoutes = new List<Route>();
        public List<Route> exportRoutes = new List<Route>();
        public string thenDuration;
        public string nowDuration;
        public string thenMiningProcess;
        public string nowMiningProcess;
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
        //public List<BookmarkItem> exportedGoods = new List<BookmarkItem>();//Only Tab app
        //public List<BookmarkItem> importedGoods = new List<BookmarkItem>();//Only Tab app
    }
    [Serializable]
    public class Hotspot {
        public string hotspotName;
        public string hotspotThumbnailPath;
        public Position position;
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
    }
}