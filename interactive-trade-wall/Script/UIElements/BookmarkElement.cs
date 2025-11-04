using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static InteractiveTradeWallDataSO;

public class BookmarkElement : MonoBehaviour
{
    private BookController controller;
    public int _id;
    MegaBookBuilder book;
    [SerializeField] private Bookmark bookmark;
    [SerializeField] private Toggle bookmarkToggle;

    [SerializeField] private TMP_Text bookmarkTitleUI;

    public void setupData(Bookmark _bm)
    {
        controller = BookController.instance;
        book = BookController.instance.swipeController.book;
        bookmark = _bm;
        //bookmarkToggle.onValueChanged.RemoveAllListeners();
        bookmarkToggle.onValueChanged.AddListener(OnClickBookmark);
        bookmarkToggle.group = GetComponentInParent<ToggleGroup>();

        if (bookmarkTitleUI != null)
            bookmarkTitleUI.text = bookmark.title;

        gameObject.name = bookmark.title;
    }
    public void OnClickBookmark(bool _ison)
    {
        if (_ison)
        {
            controller._lastClickedBookMark = bookmark;
            controller.currentSelectedBookMarkId = _id;
            StartCoroutine(BookMarkClickCoroutine());
        }
    }

    private IEnumerator BookMarkClickCoroutine()
    {
        Debug.Log($"Opening Material List");
        yield return StartCoroutine(controller.GotoPage(bookmark.pageNumber, () =>
        {
            controller.OpenMatrialPanel();
            controller.bookmarkItemsPagination.ShowBookmarkItems(bookmark);//fill data
        }));

        Debug.Log("Bookmark clicked: " + bookmark.title);
    }
}
