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

    void OnEnable()
    {
        BookController.instance.onToggleLangugae += RefreshLanguage;
    }

    void OnDisable()
    {
        BookController.instance.onToggleLangugae -= RefreshLanguage;
    }

    public void setupData(Bookmark _bm)
    {
        controller = BookController.instance;
        book = BookController.instance.swipeController.book;
        bookmark = _bm;
        //bookmarkToggle.onValueChanged.RemoveAllListeners();
        bookmarkToggle.onValueChanged.AddListener(OnClickBookmark);
        bookmarkToggle.group = GetComponentInParent<ToggleGroup>();

        if (bookmarkTitleUI != null)
        {
            if (controller.language == Language.English)
            {
                bookmarkTitleUI.font = controller.englishTmpFont;
                bookmarkTitleUI.text = bookmark.title;
            }
            else if (controller.language == Language.Marathi)
            {
                bookmarkTitleUI.font = controller.marathiTmpFont;
                bookmarkTitleUI.text = bookmark.title_marathi;
            }
        }


        gameObject.name = bookmark.title;
    }

    void RefreshLanguage()
    {
        Debug.Log("refresh Called");
       if (controller.language == Language.English)
            {
                bookmarkTitleUI.font = controller.englishTmpFont;
                bookmarkTitleUI.text = bookmark.title;
            }
            else if (controller.language == Language.Marathi)
            {
                bookmarkTitleUI.font = controller.marathiTmpFont;
                bookmarkTitleUI.text = bookmark.title_marathi;
            }
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
