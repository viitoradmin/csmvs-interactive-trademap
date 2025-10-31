using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PanelHandler : MonoBehaviour
{
    [Header("Reference to your book script")]
    public MegaBookBuilder book; // replace MegaBook with your actual book class type
    public CanvasGroup canvasGroup; // CanvasGroup attached to the root canvas
    public List<GameObject> panels = new List<GameObject>();

    private int lastActiveIndex = -1;
    private float pageValue;
    private int pageIndex;
    private void Start()
    {
        SetCanvasInteractive(false);
    }
    private void Update()
    {
        //CheckIndex();
    }

    public void CheckIndex()
    {
        if (book == null || panels.Count == 0) return;

        pageValue = book.page;
        pageIndex = Mathf.FloorToInt(pageValue);

        // Check if the value is whole number
        if (pageIndex >= 0 && pageIndex < panels.Count)
        {
            if (Mathf.Approximately(pageValue, pageIndex))
            {
            // Page is a whole number
            SetCanvasInteractive(true);

                ActivatePanel(pageIndex);
            }
            else
            {
            // Fractional page → disable interaction but keep panels as-is
            SetCanvasInteractive(false);
            }
        }
        else
        {
                // Outside range → no panel active
                DeactivateAllPanels();
        }
    }

    private void ActivatePanel(int index)
    {
        if (index < 0 || index >= panels.Count) return;

        if (lastActiveIndex == index) return;

        for (int i = 0; i < panels.Count; i++)
        {
            panels[i].SetActive(i == index);
        }

        lastActiveIndex = index;
    }

    private void DeactivateAllPanels()
    {
        if (lastActiveIndex == -1) return;

        for (int i = 0; i < panels.Count; i++)
        {
            panels[i].SetActive(false);
        }

        lastActiveIndex = -1;
    }

    private void SetCanvasInteractive(bool state)
    {
        if (canvasGroup == null) return;
        canvasGroup.interactable = state;
        canvasGroup.blocksRaycasts = state;
    }

    #region for Testing Purpose
    public TMP_Text debugText;


    public void DebugTest(string testString)
    {
        Debug.Log(testString);
        debugText.text = testString;
    }
    #endregion for Testing Purpose
}
