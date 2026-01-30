using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using ViitorCloud.Utility.PopupManager;
public struct Extension {
    public const string mp4 = ".mp4";
    public const string jpg = ".jpg";
    public const string jpeg = ".jpeg";
    public const string png = ".png";
    public const string json = ".json";
}

[Serializable]
public class MediaList {
    public Dictionary<string,Media> list;
    public MediaList() {
        list = new Dictionary<string, Media>();
    }
    public Media GetMedia(string fileName) {
        Media media = null;
        list.TryGetValue(fileName, out media);
        return media;
    }    
}
[Serializable]
public class Media {
    public string fileName;
    public int id;
    public string fileType;
    public string url;

    public bool IsVideoFile() {
        return fileType == Extension.mp4;
    }

    public bool IsImageFile() {
        return fileType == Extension.jpg;
    }
}//MediaInformation class end

[System.Serializable]
public class ImagesListData {
    public List<string> listOfImages;
}

public class MediaManager:MonoBehaviour {
    private List<Task> downloadTasks = new List<Task>();
    public MediaList _mediaList = new MediaList();
    [SerializeField] private List<string> assetURLList;
    [SerializeField] private TextAsset db;
    
    private void Start() {
        if (db) {
            ImagesListData imageList = JsonUtility.FromJson<ImagesListData>(db.text);

            if (imageList != null) {
                assetURLList = imageList.listOfImages;
                DownloadMediaFilesAsync();
            } else {
                Debug.LogError("Failed to parse JSON. Make sure class names match JSON keys.");
            }
        }
    }
    internal void AssignDownloadableUrl(List<string> downloadUrl) {
        assetURLList.Clear();
        assetURLList.AddRange(downloadUrl);
    }
   
    public async void DownloadMediaFilesAsync(Action onDownloadCompleted = null) {
        if (assetURLList == null || assetURLList.Count == 0) {
            return;
        }
        //LoadingPage Loading 0%...
        Debug.Log("Loading: 0%");
        if (PopupManager.Instance) { 
            PopupManager.Instance.ShowLoading();
        }

        downloadTasks.Clear();
        _mediaList.list.Clear();

        for (int i = 0;i < assetURLList.Count;i++) {
            downloadTasks.Add(DownloadUtility.DownloadAssetAsync(assetURLList[i],
                GetDirectoryPath(),
                OnDownloadComplete,
                OnDownloadFail,
                DownloadingProgress));
        }

        await Task.WhenAll(downloadTasks);
        AllMediaDownloaded();
        onDownloadCompleted?.Invoke();
    }
    private string GetDirectoryPath() {
        string path = Path.Combine(Application.persistentDataPath,"Images");

        if (!Directory.Exists(path)) {
            Directory.CreateDirectory(path);
        }
        return path;
    }
    private void OnDownloadComplete(string filePath) {
        //Debug.Log($"File: {Path.GetFileName(filePath)} Downloaded. \n Path: {filePath}");

        Media media = new Media() {
            fileName = Path.GetFileName(filePath),
            fileType = Path.GetExtension(filePath),
            url = filePath,
        };

        _mediaList.list[media.fileName] = media;
    }

    private void OnDownloadFail(string errorMessage) {
        Debug.LogError($"Error OnDownloadFail: {errorMessage}");
    }

    private void DownloadingProgress(float progress) {
        //LoadingPage.instance.SetProgressBar(true);

        string progressText = $"Loading: {_mediaList.list.Count}/{assetURLList.Count} {(progress * 100f).ToString("00.00")}%";
        Debug.Log(progressText);
        //LoadingPage.instance.SetProgressText(progressText);

        //LoadingPage.instance.SetProgressBarValue(progress * 100f);
    }
    private void AllMediaDownloaded() {        
        //Debug.Log("AllMediaDownloaded: Loading: 100%");
        if (PopupManager.Instance) { 
            PopupManager.Instance.HideLoading();
        }
    }
    public void GetSpriteFromResource(string assetURL,Action<Sprite> onDownloadCompleted = null) { 
        Sprite loadedSprite = Resources.Load<Sprite>(assetURL);
        onDownloadCompleted?.Invoke(loadedSprite);
    }
    public async void DownloadSingleMediaFileAsync(string assetURL,Action<Sprite> onDownloadCompleted = null) {
        if (string.IsNullOrEmpty(assetURL)) {
            Debug.LogError("Asset URL is null or empty");
            onDownloadCompleted?.Invoke(null);
            return;
        }

        // 2. Prepare Lists
        downloadTasks.Clear();
        //_mediaList.list.Clear();

        // 3. Start Download Task using the provided DownloadUtility
        Task downloadTask = DownloadUtility.DownloadAssetAsync(
            assetURL,
            GetDirectoryPath(),
            OnDownloadComplete, // This populates _mediaList
            OnDownloadFail,
            DownloadingProgress
        );
        downloadTasks.Add(downloadTask);

        // 4. Await Completion
        await Task.WhenAll(downloadTasks);

        // 5. Cleanup UI
        AllMediaDownloaded();

        // 6. Load Sprite from Disk
        Sprite resultSprite = null;

        // Replicate DownloadUtility's naming logic to find the file key
        string fileName = Path.GetFileName(assetURL).Replace("%20"," ");

        if (_mediaList.list.ContainsKey(fileName)) {
            string filePath = _mediaList.list[fileName].url;

            if (File.Exists(filePath)) {
                // Read bytes and create texture
                byte[] fileData = File.ReadAllBytes(filePath);
                Texture2D texture = new Texture2D(2,2);

                // LoadImage auto-resizes the texture dimensions
                if (texture.LoadImage(fileData)) {
                    resultSprite = Sprite.Create(
                        texture,
                        new Rect(0,0,texture.width,texture.height),
                        new Vector2(0.5f,0.5f)
                    );
                }
            } else {
                Debug.LogError($"File not found at path: {filePath}");
            }
        } else {
            Debug.LogError($"File {fileName} not found in _mediaList after download.");
        }

        // 7. Invoke Callback
        onDownloadCompleted?.Invoke(resultSprite);
    }
    internal void ClearLocalStoredData() {
        // 1. Clear the in-memory dictionary references
        _mediaList.list.Clear();
        
        // 2. Get the path where images are stored
        string path = GetDirectoryPath();

        // 3. Check if directory exists and delete it
        if (Directory.Exists(path)) {
            try {
                // The 'true' parameter performs a recursive delete 
                // (removes the folder AND all files inside it)
                Directory.Delete(path, true);
                Debug.Log($"<color=red>Successfully cleared local data at:</color> {path}");
            }
            catch (Exception e) {
                Debug.LogError($"Failed to clear local data: {e.Message}");
            }
        } else {
            Debug.LogWarning("ClearLocalStoredData: Directory did not exist, nothing to delete.");
        }
    }
}//MediaManager class end.
