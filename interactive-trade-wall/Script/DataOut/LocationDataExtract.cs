using System;
using System.Collections.Generic;
using UnityEngine;

public class LocationDataExtract : MonoBehaviour
{
    [ContextMenu("Extract")]
    public void Extract()
    {
        MaterialRoutesRoot root = new MaterialRoutesRoot();
        foreach (Transform child in transform)
        {
            root.materialRoutes = new List<MaterialRoute>();

            MaterialRoute route = new MaterialRoute();
            route.title = child.name;

            route.locations = new List<Location>();

            foreach (Transform grandchild in child)
            {
                Transform location = grandchild.GetChild(1);
                LanguageUpdate lan = location.GetComponent<LanguageUpdate>();

                // lan.LocalTextList;
                route.locations.Add(new Location(lan.LocalTextList[0].message, lan.LocalTextList));
            }
            root.materialRoutes.Add(route);
            Debug.Log(JsonUtility.ToJson(root));
        }
    }
} //LocationDataExtract class end.

[Serializable]
public class MaterialRoutesRoot
{
    public List<MaterialRoute> materialRoutes;
}

[Serializable]
public class MaterialRoute
{
    public string title;
    public List<Location> locations;
}

[Serializable]
public class Location
{
    public string locationName;
    public List<LanguageTextPair> languageTextPairs;
    
    public Location(string locationName,List<LanguageTextPair> languageTextPairs)
    {
        this.locationName = locationName;
        this.languageTextPairs = languageTextPairs;
    }
}