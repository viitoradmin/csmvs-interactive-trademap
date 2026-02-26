using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using ViitorCloud.Utility.PopupManager;
using static InteractiveTradeWallDataSO;
using static ViitorCloud.API.StandardTemplates.LoginResponse;

[Serializable]
public class LoginData {
    public string email;
    public string password;
}


[Serializable]
public class LoginResponse {
    public string message;
    public string access_token;
    public string token_type;
    public User user;
}

public class APIHandler:MonoBehaviour {
    [SerializeField] private InteractiveTradeWallDataSO dataSO;
    public Root data;
    public static Action<Root> OnDataFetchedEvent;

    // Define unique filenames for each language
    private const string FILE_NAME_ENGLISH = "TradeMapData.json";
    private const string FILE_NAME_MARATHI = "TradeMapData_Marathi.json";
    [SerializeField]
    private LoginData loginData = new LoginData() {
        email = "admin@csmvs.in",
        password = "A9$fK2@qM!"
    };
    public bool useOfflineFile = false;
    [SerializeField] private MediaManager mediaManager;

    private void Start() {
        // Only call Login first. The other calls will happen AFTER login succeeds.
        if (useOfflineFile) {
            LanguageManager.Instance.CurrentLanguage = Language.English;
        } else {
            Login();
        }
    }

    private void Login() {
        if (loginData == null) {
            loginData.email = "admin@csmvs.in";
            loginData.password = "A9$fK2@qM!";
        }

        string formData = JsonUtility.ToJson(loginData);
        PopupManager.Instance.ShowLoading();
        APICall.Instance.RequestLogin(formData,API.APILogin,
            (response) => {
                PopupManager.Instance.HideLoading();
                Debug.Log("Login Success");

                if (response.data != null) {
                    string token = response.data.access_token;
                    Debug.Log("Token: " + token);

                    // 1. STORE THE TOKEN globally in your ServerCommunication script
                    ViitorCloud.API.ServerCommunication.ViitorCloudToken = token;

                    // 2. NOW call the next API, because we have the token
                    LanguageManager.Instance.CurrentLanguage = Language.English;
                } else {
                    Debug.LogError("Response data is null");
                }
            },
            (error) => {
                Debug.LogError("Login Failed: " + error);
                PopupManager.Instance.HideLoading();
                PopupManager.Instance.ShowToast("Login Failed");
            });
    }
    private bool IsFileExistsInLocal(string fileName) {
        string filePath = Path.Combine(Application.persistentDataPath,fileName);
        return File.Exists(filePath);
    }
    internal void GetDataForEnglish() {
        if (useOfflineFile) {
            LoadOfflineDataForFileName("TradeMapData");
            return;
        }
        string filePath = Path.Combine(Application.persistentDataPath,FILE_NAME_ENGLISH);

        // 1. Check if file is available in persistent data path
        if (File.Exists(filePath)) {
            try {
                string json = File.ReadAllText(filePath);
                Root cachedData = JsonUtility.FromJson<Root>(json);

                if (cachedData != null) {
                    DownloadAllMediaAndProceed(cachedData,() => {
                        ProcessFinalData(cachedData);
                    });
                    return; // Exit here, do not call API
                }
            } catch (Exception e) {
                Debug.LogError("Error loading local file, falling back to API: " + e.Message);
            }
        }

        // 2. If no file, call API
        Debug.Log("API URL: " + API.APIGetTradeRouteSO_En);
        PopupManager.Instance.ShowLoading();
        APICall.Instance.RequestTradeRouteData(API.APIGetTradeRouteSO_En,
            (response) => {
                // Get the raw Root object
                Root rootData = response.data;

                if (rootData != null) {
                    // Save to local file for next time
                    SaveToCache(rootData,filePath);
                    ExternalCallTosaveMarathiData();
                    DownloadAllMediaAndProceed(rootData,() => {
                        ProcessFinalData(rootData);
                    });
                } else {
                    Debug.LogError("Response data is null");
                }
            },
            (error) => {
                PopupManager.Instance.HideLoading();
                PopupManager.Instance.ShowToast("Failed To Load English Data");
                Debug.LogError("Fail: " + error);
            });
    }

    internal void GetDataForMarathi() {
        if (useOfflineFile) {
            LoadOfflineDataForFileName("TradeMapData_Marathi");
            return;
        }
        string filePath = Path.Combine(Application.persistentDataPath,FILE_NAME_MARATHI);

        // 1. Check if file is available in persistent data path
        if (File.Exists(filePath)) {
            try {
                string json = File.ReadAllText(filePath);
                Root cachedData = JsonUtility.FromJson<Root>(json);

                if (cachedData != null) {
                    // Apply Marathi specific conversion
                    cachedData.Convert(LanguageManager.Instance);
                    DownloadAllMediaAndProceed(cachedData,() => {
                        ProcessFinalData(cachedData);
                    });
                    return; // Exit here, do not call API
                }
            } catch (Exception e) {
                Debug.LogError("Error loading local file, falling back to API: " + e.Message);
            }
        }

        Debug.Log("API URL: " + API.APIGetTradeRouteSO_Mr);
        PopupManager.Instance.ShowLoading();
        APICall.Instance.RequestTradeRouteData(API.APIGetTradeRouteSO_Mr,
            (response) => {
                // Get the raw Root object
                Root rootData = response.data;

                if (rootData != null) {
                    // Save RAW data to local file (before conversion)
                    SaveToCache(rootData,filePath);
                    rootData.Convert(LanguageManager.Instance);
                    DownloadAllMediaAndProceed(rootData,() => {
                        ProcessFinalData(rootData);
                    });
                } else {
                    Debug.LogError("Response data is null");
                }
            },
            (error) => {
                PopupManager.Instance.HideLoading();
                PopupManager.Instance.ShowToast("Failed To Load Marathi Data");
                Debug.LogError("Fail: " + error);
            });
    }
    private void ExternalCallTosaveMarathiData() {
        string filePath = Path.Combine(Application.persistentDataPath,FILE_NAME_MARATHI);
        APICall.Instance.RequestTradeRouteData(API.APIGetTradeRouteSO_Mr,
           (response) => {
               // Get the raw Root object
               Root rootData = response.data;

               if (rootData != null) {
                   // Save RAW data to local file (before conversion)
                   SaveToCache(rootData,filePath);
               }
           },
           (error) => {
               Debug.LogError("Fail: " + error);
           });
    }
    private void LoadOfflineDataForFileName(string fileName) {
        TextAsset jsonData = Resources.Load<TextAsset>($"OfflineData/{fileName}");

        if (jsonData != null) {
            Root offlineData = JsonUtility.FromJson<Root>(jsonData.text);
            if (offlineData != null) {
                if (LanguageManager.Instance.CurrentLanguage.Equals(Language.Marathi)) {
                    offlineData.Convert(LanguageManager.Instance);
                }
                ProcessFinalData(offlineData);
            }
        }
    }

    // Helper method to handle assignment logic
    private void ProcessFinalData(Root rootData) {
        data = rootData;
        dataSO.root = rootData;
        OnDataFetchedEvent?.Invoke(rootData);
        
        StartCoroutine(HideLoading());
    }

    private IEnumerator HideLoading(){
        yield return new WaitForSeconds(0.8f);
        PopupManager.Instance.HideLoading();
    }

    // Helper method to save JSON to file
    private void SaveToCache(Root rootData,string path) {
        try {
            string json = JsonUtility.ToJson(rootData,true);
            File.WriteAllText(path,json);
            Debug.Log("Data saved to: " + path);
        } catch (Exception e) {
            Debug.LogError("Failed to save data to cache: " + e.Message);
        }
    }
    private void DownloadAllMediaAndProceed(Root root,Action OnDonwnloadCompleted) {
        List<string> allDownloadUrl = new List<string>();
        root.CollectAllImagePaths(allDownloadUrl);
        mediaManager.AssignDownloadableUrl(allDownloadUrl);
        mediaManager.DownloadMediaFilesAsync(OnDonwnloadCompleted);
    }
    internal void ClearLocalStoredData() {
        string filePathEn = Path.Combine(Application.persistentDataPath,FILE_NAME_ENGLISH);
        string filePathMr = Path.Combine(Application.persistentDataPath,FILE_NAME_MARATHI);
        if (File.Exists(filePathEn)) {
            File.Delete(filePathEn);
            Debug.Log("Deleted cached English data file.");
        }
        if (File.Exists(filePathMr)) {
            File.Delete(filePathMr);
            Debug.Log("Deleted cached Marathi data file.");
        }
    }
}//APIHandler class end.