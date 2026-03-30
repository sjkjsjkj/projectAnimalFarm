using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IPointerClickHandler
{
    public Image icon;
    public TextMeshProUGUI countText;

    private Item item;
    private int count;

    public void SetItem(Item newItem, int newCount)
    {
        item = newItem;
        count = newCount;

        icon.sprite = item.icon;
        icon.enabled = true;

        countText.text = count.ToString();
    }

    public void ClearSlot()
    {
        item = null;
        count = 0;

        icon.sprite = null;
        icon.enabled = false;

        countText.text = "";
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (item == null) return;

        ItemPopupUI.instance.Open(this);
    }

    public Item GetItem()
    {
        return item;
    }

    public void RemoveItem()
    {
        ClearSlot();
    }
}
