using System;
using System.IO;
using UnityEngine;

[CreateAssetMenu(fileName = "TVScreenDataSO", menuName = "Scriptable Objects/TVScreenDataSO")]
public class TVScreenDataSO : ScriptableObject
{

    public TextAsset textAsset;
    public TVScreenDataRoot tvScreenDataRoot;

    [ContextMenu("Load JSON to ScriptableObject")]
    public void LoadFromJson()
    {
        if (textAsset == null)
        {
            Debug.LogError("No json assigned");
            return;
        }
        tvScreenDataRoot = JsonUtility.FromJson<TVScreenDataRoot>(textAsset.text);
        Debug.Log("Scriptable Object updated for TVScreenData");
    }

    [ContextMenu("Save ScriptableObject to JSON")]
    public void SaveToJson()
    {
        // Convert ScriptableObject to JSON
        string json = JsonUtility.ToJson(tvScreenDataRoot, true);

        // Write file
        File.WriteAllText(textAsset.text, json);

        Debug.Log("JSON saved.");
    }

    [Serializable]
    public class TVScreenDataRoot
    {
        public TVScreenBookMark[] bookMarks;
    }

    [Serializable]
    public class TVScreenBookMark
    {
        public int id;
        public TVScreenBookMarkItem[] items;
    }

    [Serializable]
    public class TVScreenBookMarkItem
    {
        public int id;
        public TVScreenRouteDetails importRoute;
        public TVScreenRouteDetails exportRoute;
    }
    
    [Serializable]
    public class TVScreenRouteDetails
    {
        public string routeImagePath;
        public string[] hotspotImagePath;
    }
}
