using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Mapping {
    public string hindi;
    public string transliteration;
}

[System.Serializable]
public class LanguageReader {
    public List<Mapping> mappings;
}

public class MarathiToVakraReader:MonoBehaviour {
    [SerializeField] private TextAsset jsonFile; // Assign the JSON file in the Unity Inspector

    private Dictionary<string,string> marathiToVakraMap;
    public Dictionary<string,string> MarathiToVakraMap {
        get { return marathiToVakraMap; }
    }

    void Awake() {
        if (jsonFile == null) {
            Debug.LogError("JSON file not assigned!");
            return;
        }

        marathiToVakraMap = new Dictionary<string,string>();
        LoadJson();
    }

    void LoadJson() {
        try {
            // Deserialize JSON into the wrapper class
            LanguageReader wrapper = JsonUtility.FromJson<LanguageReader>(jsonFile.text);

            // Convert the list of key-value pairs into a Dictionary<char, string>
            foreach (Mapping entry in wrapper.mappings) {
                if (!string.IsNullOrWhiteSpace(entry.hindi)) {
                    string key = entry.hindi;
                    marathiToVakraMap[key] = entry.transliteration;
                } else {
                    Debug.LogWarning($"Invalid mapping skipped: {entry.hindi}");
                }
            }

            Debug.Log($"Loaded {marathiToVakraMap.Count} mappings.");

            // Example: Test a mapping
            //if (marathiToVakraMap.ContainsKey("क")) {
            //    Debug.Log($"Mapping for 'क': {marathiToVakraMap["क"]}");
            //}
        } catch (System.Exception e) {
            Debug.LogError($"Error loading JSON: {e.Message}");
        }
    }

    public string GetVakra(string marathiText) {
        string _map = string.Empty;
        if (marathiToVakraMap.ContainsKey(marathiText)) {
            _map = marathiToVakraMap[marathiText];
            //Debug.Log($"Mapping for {marathiText}: {_map}");
        } else if(string.IsNullOrWhiteSpace(marathiText)) {            
            _map = " ";
        }
        return _map;
    }
}