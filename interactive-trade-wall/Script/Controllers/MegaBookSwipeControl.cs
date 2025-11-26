using UnityEngine;
using UnityEngine.EventSystems;

public class MegaBookSwipeControl : MonoBehaviour
{
    public MegaBookBuilder book;

    [Header("Swipe Settings")]
    public float swipeSensitivity = 0.005f;  // smaller = faster page turn
    public float snapSpeed = 10f;            // snapping speed

    private Vector2 startPos;
    private bool isDragging = false;
    private bool isLeftToRight = false;

    [SerializeField] private float startPage;   // where swipe begins
    [SerializeField] private float endPage;     // target page for this swipe
    [SerializeField] private float targetPage;  // final snap target

    [SerializeField] private PanelHandler panelHandler;
    public int returnPageNumber = -1;
    void Start()
    {
        InitBook();
    }

    void Update()
    {
        if (book == null || IsMouseOverUI()) return;

        HandleInput();
        //HandleSnapping();
        if (Input.GetKeyDown(KeyCode.L))
        {
            ToggleCubeSize();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Rebuild();
        }
    }

    // -------------------- CORE METHODS --------------------

    void InitBook()
    {
        if (book != null)
        {
            targetPage = Mathf.Clamp(Mathf.Round(book.page), -1, book.MaxPageVal());
            book.page = targetPage;
        }
    }

    void HandleInput()
    {
       // if (Input.GetMouseButtonDown(0))
           // StartSwipe();

        //if (isDragging && Input.GetMouseButton(0))
            //UpdateSwipe();

        //if (isDragging && Input.GetMouseButtonUp(0))
            //EndSwipe();
    }

    void StartSwipe()
    {
        startPos = Input.mousePosition;
        isDragging = true;

        startPage = Mathf.Round(book.page);
        startPage = Mathf.Clamp(startPage, -1, book.MaxPageVal());

        endPage = startPage; // provisional, will change during drag
    }

    void UpdateSwipe()
    {
        Vector2 delta = (Vector2)Input.mousePosition - startPos;
        float dragNorm = delta.x * -swipeSensitivity; // normalized drag amount

        if (dragNorm > 0)
        { // left → right swipe
            isLeftToRight = true;
            endPage = Mathf.Clamp(startPage + 1, -1, book.MaxPageVal());
        }
        else if (dragNorm < 0)
        { // right → left swipe
            isLeftToRight = false;
            endPage = Mathf.Clamp(startPage - 1, -1, book.MaxPageVal());
        }

        float t = Mathf.Clamp01(Mathf.Abs(dragNorm));
        book.page = Mathf.Lerp(startPage, endPage, t);
    }

    void EndSwipe()
    {
        isDragging = false;

        float mid = (startPage + endPage) * 0.5f;
        if (isLeftToRight)
        {
            targetPage = (book.page > mid) ? endPage : startPage;
        }
        else
        {
            targetPage = (book.page > mid) ? startPage : endPage;
        }
        book.page = targetPage;
    }

    public void NextPage()
    {
        book.page = Mathf.Clamp((int)book.page + 1, -1, book.MaxPageVal());
    }

    public void PreviousPage()
    {
        book.page = Mathf.Clamp((int)book.page - 1, -1, book.MaxPageVal());
    }

    public static bool IsMouseOverUI()
    {
        // Works for mouse pointer
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }
    public int pagenumber;
    public Transform cube;
    [ContextMenu("GoToPage")]
    public void GoToPage()
    {
        book.page = Mathf.Clamp((int)pagenumber, -1, book.MaxPageVal());
    }
    public void GoToPage(int _page_num)
    {
        book.page = Mathf.Clamp((int)_page_num, -1, book.MaxPageVal());
    }

    public void CloseBook()
    {
        book.page = -1;
    }
    [ContextMenu("Rebuild")]
    public void Rebuild()
    {
        book.rebuild = true;
    }

    public void ToggleCubeSize()
    {
        if (cube.localScale == Vector3.one)
        {
            cube.localScale = Vector3.one * 5;
        }
        else
        {
            cube.localScale = Vector3.one;
        }
    }
}
