using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;

public class ImageLoaderFromURL : MonoBehaviour
{
    // Optional: set a default PPU that matches your project
    public float pixelsPerUnit = 100f;
    public static ImageLoaderFromURL Instance { get; private set; }
    void Awake()
    {
        //PhotonNetwork.AutomaticallySyncScene = true;
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Load a public Google Drive image and assign it to a SpriteRenderer.
    /// </summary>
    public void LoadSpriteFromGoogleDrive(string driveUrl, SpriteRenderer target)
    {
        StartCoroutine(CoLoadSprite(driveUrl, target));
    }

    IEnumerator CoLoadSprite(string driveUrl, SpriteRenderer target)
    {
        if (target == null)
        {
            Debug.LogError("[DriveImageLoader] Target SpriteRenderer is null.");
            yield break;
        }

       // If it's a Google Drive link, convert it; otherwise use it as-is
string url = driveUrl;
if (driveUrl.Contains("drive.google.com"))
{
    string fileId = ExtractFileId(driveUrl);
    if (!string.IsNullOrEmpty(fileId))
        url = $"https://drive.google.com/uc?export=download&id={fileId}";
}

        using (UnityWebRequest req = UnityWebRequestTexture.GetTexture(url))
        {
            req.timeout = 15; // seconds
            yield return req.SendWebRequest();

#if UNITY_2020_2_OR_NEWER
            if (req.result != UnityWebRequest.Result.Success)
#else
            if (req.isNetworkError || req.isHttpError)
#endif
            {
                Debug.LogError("[DriveImageLoader] Download failed: " + req.error + " | " + url);
                yield break;
            }

            Texture2D tex = DownloadHandlerTexture.GetContent(req);
            if (tex == null)
            {
                Debug.LogError("[DriveImageLoader] No texture content.");
                yield break;
            }

            // Optional tuning
            tex.wrapMode   = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;

            // Build a sprite and assign
            var rect = new Rect(0, 0, tex.width, tex.height);
            var pivot = new Vector2(0.5f, 0.5f);
            Sprite sprite = Sprite.Create(tex, rect, pivot, pixelsPerUnit);
            target.sprite = sprite;
        }
    }

    /// <summary>
    /// Supports URLs like:
    ///   https://drive.google.com/file/d/FILE_ID/view?usp=sharing
    ///   https://drive.google.com/open?id=FILE_ID
    ///   https://drive.google.com/uc?id=FILE_ID&export=download
    /// </summary>
    string ExtractFileId(string url)
    {
        if (string.IsNullOrEmpty(url)) return null;

        // /d/FILE_ID/...
        var m = Regex.Match(url, @"/d/([a-zA-Z0-9_-]+)");
        if (m.Success) return m.Groups[1].Value;

        // id=FILE_ID
        m = Regex.Match(url, @"[?&]id=([a-zA-Z0-9_-]+)");
        if (m.Success) return m.Groups[1].Value;

        return null;
    }
}
