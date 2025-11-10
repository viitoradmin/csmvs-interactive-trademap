using Coffee.UIEffects;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using TMPro.EditorUtilities;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;
using static InteractiveTradeWallDataSO;

public class BookController : MonoBehaviour
{
    public MegaBookBuilder book;
    public static BookController instance;
    public Language language;
    public MegaBookSwipeControl swipeController;
    public ItemsPagination bookmarkItemsPagination;

    public int initialPageIndex = -1;
    public int materialStartPageIndex = 11;
    public int detailsStartPageIndex = 5;

    [SerializeField] private float cutoff_delta = 0.1f;

    public CanvasGroup canvasGroup;
    private GraphicRaycaster graphicRaycaster;

    List<Toggle> bookmarkToggles = new List<Toggle>();
    [Header("UI Elememnts")]
    public TMP_Text bottomline_text;
    public GameObject materialListPanel;
    public GameObject materialDetailsPanel;

    [Header("Assign BookMarks")]
    public ToggleGroup bookmark_toggleGroup;
    public Transform parentA;
    public Transform parentB;
    public GameObject bookmarkButtonPrefab;

    [Header("Details Page UI Elememnts")]
    public HorizontalSlideshow slideShowManager;
    public RawImage pinned_rawimage;

    [Space]
    public TMP_Text backButtonPath;
    public TMP_Text materialTitleText;
    public TMP_Text materialDetailsText;
    public Text materialDetailsTxt;
    [Space]
    public TMP_Text importRoutes_Button_Text;
    public TMP_Text exportRoutes_Button_Text;
    [Space]
    public TMP_Text routeFacts_Title_Text;

    public TMP_Text thenVSnow_Title_Text;
    [Space]
    public TMP_Text distance_Title_Text;
    public TMP_Text distance_Value_Text;
    public TMP_Text meritimeRoute_Title_Text;
    public TMP_Text meritimeRoute_Value_Text;
    public TMP_Text overlandRoute_Title_Text;
    public TMP_Text overlandRoute_Value_Text;
    public TMP_Text challenges_Title_Text;
    public TMP_Text challenges_Value_Text;
    [Space]
    public TMP_Text journeyDuration_Then_Text;
    public TMP_Text journeyDuration_Now_Text;
    public TMP_Text miningProcess_Then_Text;
    public TMP_Text miningProcess_Now_Text;

    public int currentSelectedBookMarkId=-1;
    public int currentSelectedItemId = -1;

    public Font englisthFont, marathiFont;
    public TMPro.TMP_FontAsset englishTmpFont, marathiTmpFont;

    [Header("UI Effects Related Data")]
    public UIEffectsController uIEffectsController;

    [Header("Sample Bookmark Data")]
    //public Data dataSO;
    public InteractiveTradeWallDataSO dataSO;
    public InteractiveTradeWallDataSO marathiDataSO;

    public UnityAction onToggleLangugae; 
    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        graphicRaycaster = canvasGroup.GetComponent<GraphicRaycaster>();
        //StartCoroutine(OpenBook());
        canvasGroup.alpha = 0.0f;
        StartCoroutine(uIEffectsController.PlayUIEffectsCoroutine(false, () =>
          {
              canvasGroup.alpha = 1.0f;
          }));
        SetupBookmarks();
        //ShowMaterials();
    }

    public void ToogleLanguge()
    {
        if (language == Language.English)
        {
            language = Language.Marathi;

        }
        else if (language == Language.Marathi)
        {
            language = Language.English;

        }
        Debug.Log("Current Page:" + book.GetCurrentPage());
        if (book.GetCurrentPage() == 6)
        {
            Debug.Log("This is detailed page section");
            SetDetailPageUI(_LastSelectedItem);
        }
        else
        {
            onToggleLangugae?.Invoke();
        }
    }

    [ContextMenu("Close book")]
    public void CloseBook()
    {
        canvasGroup.alpha = 0.0f;
        swipeController.CloseBook();
    }
    public BookmarkItem _LastSelectedItem;
    public void ShowDetails(BookmarkItem _data, Texture _materialImage)
    {
        // disable all Buttons and UI
        Debug.Log($"open data called {_data.title}");
        _LastSelectedItem = _data;
        SetReturnPage();
        pinned_rawimage.texture = _materialImage;
        StartCoroutine(GotoPage(detailsStartPageIndex, () =>
        {
            OpenDetailPanel();
            SetDetailPageUI(_data);
        }));
    }

    public IEnumerator GotoPage(int _page_num, UnityAction unityAction)
    {
        //yield return ShowCanvas(false);
        SetRaycaster(false);
        yield return uIEffectsController.PlayUIEffectsCoroutine(false);
        swipeController.GoToPage(_page_num);
        unityAction?.Invoke();
        yield return new WaitUntil(() => Mathf.Abs(swipeController.book.Flip - swipeController.book.page) < cutoff_delta);
        //yield return new WaitUntil(() => swipeController.book.Flip == swipeController.book.page);
        //yield return new WaitForSeconds(.5f);

        //yield return ShowCanvas(true);
        yield return uIEffectsController.PlayUIEffectsCoroutine(true);
        SetRaycaster(transform);


    }
    private void SetRaycaster(bool _enable)
    {
        graphicRaycaster.enabled = _enable;
    }

    public void SetupBookmarks()
    {
        if (bookmarkButtonPrefab == null || parentA == null || parentB == null)
        {
            Debug.LogError("Setup missing in BookmarkDistributor!");
            return;
        }

        // Clean old children
        foreach (Transform child in parentA) Destroy(child.gameObject);
        foreach (Transform child in parentB) Destroy(child.gameObject);
        bookmarkToggles.Clear();

        // Distribute bookmarks equally
        
        
            int mid = Mathf.CeilToInt(dataSO.root.bookmarks.Count / 2);
            for (int i = 0; i < dataSO.root.bookmarks.Count; i++)
            {
                Transform targetParent = (i < mid) ? parentA : parentB; // alternate distribution
                GameObject toggleObj = Instantiate(bookmarkButtonPrefab, targetParent);

                Toggle _toggle = toggleObj.GetComponent<Toggle>();
                BookmarkElement _bookmarkElement = toggleObj.GetComponent<BookmarkElement>();
                _bookmarkElement._id = i;
                int _index = i;
                if (_bookmarkElement != null)
                {
                    _bookmarkElement.setupData(dataSO.root.bookmarks[_index]);
                }
                bookmarkToggles.Add(_toggle);
            }
        

        bookmarkToggles[0].isOn = true;
    }

    public IEnumerator ShowCanvas(bool _show)
    {
        float alpha = _show ? 1.0f : 0.0f;
        if (_show)
        {
            while (canvasGroup.alpha < alpha)
            {
                canvasGroup.alpha += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
            graphicRaycaster.enabled = true;
        }
        else
        {
            graphicRaycaster.enabled = false;
            while (canvasGroup.alpha > alpha)
            {
                canvasGroup.alpha -= Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
        }
        canvasGroup.alpha = alpha;
    }
    public Bookmark _lastClickedBookMark;
    public void OnClickBackButton()
    {
        StartCoroutine(GotoPage(swipeController.returnPageNumber, () =>
        {
            OpenMatrialPanel();
            
            bookmarkItemsPagination.ShowBookmarkItems(_lastClickedBookMark);
        }));

        // TODO : Code in connection manager for moving back.
        ConnectionManager.Instance.RaiseEventForBackButtonClick();
    }

    public void OpenMatrialPanel()
    {
        materialListPanel.SetActive(true);
        materialDetailsPanel.SetActive(false);
    }
    public void OpenDetailPanel()
    {
        materialListPanel.SetActive(false);
        materialDetailsPanel.SetActive(true);
    }
    public void SetReturnPage()
    {
        swipeController.returnPageNumber = (int)Mathf.Clamp((int)swipeController.book.page, -1, swipeController.book.MaxPageVal()); ;
    }
    public void OnClickNextPage()
    {
        StartCoroutine(GotoPage((int)swipeController.book.page + 1, () =>
        {
            bookmarkItemsPagination.NextPage();
        }));
    }
    public void OnClickPreviousPage()
    {
        StartCoroutine(GotoPage((int)swipeController.book.page - 1, () =>
        {
            bookmarkItemsPagination.NextPage();
        }));
    }
    public MarathiTextParser marathiParser;
   

    public void SetDetailPageUI(BookmarkItem itemData)
    {
        backButtonPath.text = bookmarkItemsPagination.currentSelectedBookmark.title + " > " + itemData.title;
        materialTitleText.text = itemData.bookmarkMetadata.title;
        materialDetailsText.text = language == Language.English ? itemData.bookmarkMetadata.description : marathiParser.GetMarathiText(itemData.bookmarkMetadata.description);

        if(language == Language.English)
        {
            materialDetailsTxt.font = englisthFont;
            materialDetailsTxt.text = itemData.bookmarkMetadata.description;
        }else if(language == Language.Marathi)
        {
            materialDetailsTxt.font = marathiFont;
            materialDetailsTxt.text = marathiParser.GetMarathiText(itemData.bookmarkMetadata.description_marathi);
        }
        

        importRoutes_Button_Text.text = language == Language.English ? "Import Routes" : "Āyāta mārga";
        exportRoutes_Button_Text.text = language == Language.English ? "Export Routes" : "Niryāta mārga";
        routeFacts_Title_Text.text = language == Language.English ? "Route Facts" : "Mārga tathyē";
        thenVSnow_Title_Text.text = language == Language.English ? "Then vs Now" : "Maga ātā vi";
        distance_Title_Text.text = language == Language.English ? "Distance" : "Antara";
        distance_Value_Text.text = itemData.bookmarkMetadata.distance;
        meritimeRoute_Title_Text.text = language == Language.English ? "Meritime Route" : "Sāgarī mārga";
        meritimeRoute_Value_Text.text = itemData.bookmarkMetadata.meritimeRoute;
        overlandRoute_Title_Text.text = language == Language.English ? "Overland Route" : "Jaminīvaracā mārga\r\n";
        overlandRoute_Value_Text.text = itemData.bookmarkMetadata.overlandRoute;
        challenges_Title_Text.text = language == Language.English ? "Challenges" : "Āvhānē";
        challenges_Value_Text.text = itemData.bookmarkMetadata.challenges;
        journeyDuration_Then_Text.text = itemData.bookmarkMetadata.thenDuration;
        journeyDuration_Now_Text.text = itemData.bookmarkMetadata.nowDuration;
        miningProcess_Then_Text.text = itemData.bookmarkMetadata.thenMiningProcess;
        miningProcess_Now_Text.text = itemData.bookmarkMetadata.nowMiningProcess;

        //set slideshow
        slideShowManager.SetupSlides(itemData.bookmarkMetadata.images);
        SetBottomLine(false);
    }

    public void SetBottomLine(bool isMaterialPage)
    {
        bottomline_text.text = isMaterialPage ? dataSO.root.materialPageBottomLine : dataSO.root.detailPageBottomLine;
    }

    public void LoadImageFromURL(string url, RawImage rawImage)
    {
        StartCoroutine(DownloadImage(url, rawImage));
    }

    private IEnumerator DownloadImage(string url, RawImage rawImage)
    {
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url))
        {
            yield return uwr.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
            if (uwr.result != UnityWebRequest.Result.Success)
#else
            if (uwr.isNetworkError || uwr.isHttpError)
#endif
            {
                Debug.LogError("Image Load Failed: " + uwr.error);
            }
            else
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(uwr);

                // If using RawImage (UI)
                if (rawImage != null)
                    rawImage.texture = texture;

                // Or example: apply to material
                // GetComponent<Renderer>().material.mainTexture = texture;
            }
        }
    }
    public RawImage SpawnRawImage(RectTransform parent)
    {
        if (parent == null)
        {
            Debug.LogError("❌ No parent assigned for RawImage!");
            return null;
        }

        // Create GameObject
        GameObject go = new GameObject("SpawnedRawImage", typeof(RectTransform), typeof(RawImage));
        go.transform.SetParent(parent, false);

        // Setup RectTransform size same as parent
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = parent.rect.size;
        rect.localPosition = Vector3.zero;

        // Setup RawImage
        RawImage rawImage = go.GetComponent<RawImage>();

        return rawImage;
    }

}

public enum Language
{
    English, Marathi
}