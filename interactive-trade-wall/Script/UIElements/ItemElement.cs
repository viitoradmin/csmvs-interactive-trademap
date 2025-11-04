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

    public void SetupData(BookmarkItem _bookmarkItem,bool _isSelected)
    {
        m_ItemData = _bookmarkItem;
        title_text.text = m_ItemData.title;
        BookController.instance.LoadImageFromURL(m_ItemData.thumbnailPath, material_rawimage);
        itemButton.onClick.AddListener(ViewDetails);
        MarkThisItemAsSelected(_isSelected);
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
        ConnectionManager.Instance.MaterialClickedinBook(BookController.instance.currentSelectedBookMarkId,BookController.instance.currentSelectedItemId);   

        BookController.instance.ShowDetails(m_ItemData, material_rawimage.texture);
    }
    

        
}
