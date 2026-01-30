using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static InteractiveTradeWallDataSO;

public class ItemsPagination:MonoBehaviour {
    public Bookmark currentSelectedBookmark;
    [Header("Parents that hold minerals (max 4 each)")]
    public Transform itemParent1;
    public Transform itemParent2;

    [Header("Prefab for a single mineral UI element")]
    public GameObject itemPrefab;

    //[Header("Total number of minerals to paginate")]
    //public int materialsCount = 20;

    [Header("UI Elements")]
    public Button nextButton;
    public Button prevButton;
    [SerializeField] private TMP_Text leftTitle;
    [SerializeField] private TMP_Text rightTitle;

    private int currentPage = 0;
    private int mineralsPerPage = 8; // 4 in parent1 + 4 in parent2

    private int lastSelectedItemIndex = -1;
    private void Start() {
        if (nextButton != null)
            nextButton.onClick.AddListener(NextPage);
        if (prevButton != null)
            prevButton.onClick.AddListener(PrevPage);

        //LoadPage(0);
    }

    public void ShowBookmarkItems(Bookmark _bookmark) {
        currentSelectedBookmark = _bookmark;
        ResetPagination();
        LoadPage(0);
    }
    private void LoadPage(int pageIndex) {
        ClearParent(itemParent1);
        ClearParent(itemParent2);

        leftTitle.text = currentSelectedBookmark.title;
        rightTitle.text = currentSelectedBookmark.title;
        int materialsCount = currentSelectedBookmark.items.Count;
        if (materialsCount == 0) {
            return;
        }
        int startIndex = pageIndex * mineralsPerPage;
        int remaining = materialsCount - startIndex;

        if (remaining <= 0)
            return;
        // Fill parent1
        int fill1 = Mathf.Min(4,remaining);
        SpawnMinerals(itemParent1,fill1,startIndex,true);
        remaining -= fill1;

        // Fill parent2
        int fill2 = Mathf.Min(4,remaining);
        SpawnMinerals(itemParent2,fill2,startIndex + fill1,false);
        remaining -= fill2;

        // Update navigation buttons
        if (prevButton != null)
            prevButton.interactable = (pageIndex > 0);
        if (nextButton != null)
            nextButton.interactable = (materialsCount > (pageIndex + 1) * mineralsPerPage);

        BookController.instance.SetBottomLine(true);

        Debug.Log($"Page {pageIndex + 1}: showing {fill1 + fill2} minerals");
    }

    private void SpawnMinerals(Transform parent,int count,int startIndex,bool isLeft) {
        // This is to higlight the current selected element.
        if (BookController.instance != null) {
            //Debug.Log(">>" + BookController.instance._LastSelectedItem.title);
            //Debug.Log(currentSelectedBookmark.items.Count);

            for (int i = 0;i < currentSelectedBookmark.items.Count;i++) {
                if (BookController.instance._LastSelectedItem.title == currentSelectedBookmark.items[i].title) {
                    //Debug.Log("this is the item last selected" + i);
                    lastSelectedItemIndex = i;
                    break;
                } else {
                    lastSelectedItemIndex = -1;
                }
            }
        }


        for (int i = 0;i < count;i++) {
            int _index = startIndex + i;
            GameObject mineral = Instantiate(itemPrefab,parent);
            mineral.name = currentSelectedBookmark.items[_index].title;
            ItemElement _item = mineral.GetComponent<ItemElement>();

            _item._id = isLeft ? i : i + 4;
            if (lastSelectedItemIndex == _index) {
                _item.SetupData(currentSelectedBookmark.items[_index],true);
            } else {
                _item.SetupData(currentSelectedBookmark.items[_index],false);

            }

            //mineral.GetComponentInChildren<TMP_Text>().text = mineral.name;
            //LoadImageFromURL(currentSelectedBookmark.items[_index].thumbnailPath, mineral.GetComponentInChildren<RawImage>());
            //Button button = mineral.GetComponent<Button>();
            //if (button != null) { 
            //    button.onClick.AddListener(()=>BookController.instance.ShowDetails(_index));
            //}
        }
    }

    private void ClearParent(Transform parent) {
        for (int i = parent.childCount - 1;i >= 0;i--) {
            Destroy(parent.GetChild(i).gameObject);
        }
    }

    public void ResetPagination() {
        currentPage = 0;
    }
    [ContextMenu("next")]
    public void NextPage() {
        int maxPage = Mathf.CeilToInt((float)currentSelectedBookmark.items.Count / mineralsPerPage) - 1;
        if (currentPage < maxPage) {
            currentPage++;
            LoadPage(currentPage);
        }
    }
    [ContextMenu("previous")]
    public void PrevPage() {
        if (currentPage > 0) {
            currentPage--;
            LoadPage(currentPage);
        }
    }
}
