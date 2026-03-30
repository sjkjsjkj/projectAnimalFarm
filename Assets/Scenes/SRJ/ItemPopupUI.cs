using UnityEngine;
using UnityEngine.UI;

public class ItemPopupUI : MonoBehaviour
{
    public static ItemPopupUI instance;

    public GameObject popup;

    public Button useButton;
    public Button trashButton;
    public Button cancelButton;

    private InventorySlot currentSlot;

    private void Awake()
    {
        instance = this;
        popup.SetActive(false);

        useButton.onClick.AddListener(UseItem);
        trashButton.onClick.AddListener(TrashItem);
        cancelButton.onClick.AddListener(ClosePopup);
    }

    public void Open(InventorySlot slot)
    {
        currentSlot = slot;
        popup.SetActive(true);
    }

    public void ClosePopup()
    {
        popup.SetActive(false);
    }

    private void UseItem()
    {
        Debug.Log("아이템 사용!");

        currentSlot.RemoveItem();
        ClosePopup();
    }

    private void TrashItem()
    {
        Debug.Log("아이템 버림!");

        currentSlot.RemoveItem();
        ClosePopup();
    }
}
