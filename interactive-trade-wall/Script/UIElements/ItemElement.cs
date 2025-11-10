using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static InteractiveTradeWallDataSO;

public class ItemElement : MonoBehaviour
{
    public int _id;
    [SerializeField] private BookmarkItem m_ItemData;
    
    [SerializeField] private TMP_Text title_text;
    [SerializeField] private RawImage material_rawimage;

    [SerializeField] private Button itemButton;

    [SerializeField] private Image _ItemBGImage;
    [SerializeField] private Sprite _selectedBG;
    [SerializeField] private Sprite _normalBg;
    [SerializeField] private Color _selectedTextColor;
    [SerializeField] private Color _normalTextColor;

    void OnEnable()
    {
        BookController.instance.onToggleLangugae += RefreshLanguage;
    }

    void OnDisable()
    {
        BookController.instance.onToggleLangugae -= RefreshLanguage;
    }

    public void SetupData(BookmarkItem _bookmarkItem, bool _isSelected)
    {
        m_ItemData = _bookmarkItem;
        if (BookController.instance.language == Language.English)
        {
            title_text.text = m_ItemData.title;
        }
        else if (BookController.instance.language == Language.Marathi)
        {
            title_text.text = BookController.instance.marathiParser.GetMarathiText(m_ItemData.title_marathi);
        }
        //--add marathi field Here--
        BookController.instance.LoadImageFromURL(m_ItemData.thumbnailPath, material_rawimage);
        itemButton.onClick.AddListener(ViewDetails);
        MarkThisItemAsSelected(_isSelected);
    }

    void RefreshLanguage()
    {
        Debug.Log("refresh Called");
        if (BookController.instance.language == Language.English)
        {
            title_text.font = BookController.instance.englishTmpFont;
            title_text.text = m_ItemData.title;
        }
        else if (BookController.instance.language == Language.Marathi)
        {
            title_text.font = BookController.instance.marathiTmpFont;
            title_text.text = BookController.instance.marathiParser.GetMarathiText(m_ItemData.title_marathi);
        }
    }

    public void MarkThisItemAsSelected(bool _isSelected)
    {
        if (_isSelected)
        {
            _ItemBGImage.sprite = _selectedBG;
            title_text.color = _selectedTextColor;
        }
        else
        {
            _ItemBGImage.sprite = _normalBg;
            title_text.color = _normalTextColor;
        }
    }

    public void ViewDetails()
    {
        // Call for the TV Screen.
        Debug.Log("<color=green>Current Selected bookmark id is: </color>" + BookController.instance.currentSelectedBookMarkId);
        BookController.instance.currentSelectedItemId = _id;
        Debug.Log("<color=yellow>Current selected material id:</color>" + BookController.instance.currentSelectedItemId);
        //ConnectionManager.Instance.MaterialClickedinBook(BookController.instance.currentSelectedBookMarkId,BookController.instance.currentSelectedItemId);
        //ToDo: Pass the clicked material item id from here to TV Screen.   
        TVScreenManager.Instance.ShowDetailedScreen();
        BookController.instance.ShowDetails(m_ItemData, material_rawimage.texture);
    }
    

        
}
