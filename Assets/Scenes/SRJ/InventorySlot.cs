using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour, IPointerClickHandler
{
    public Image icon;
    public TextMeshProUGUI countText;

    Item item;
    int count;

    public bool IsEmpty()
    {
        return item == null;
    }

    public void SetItem(Item newItem, int newCount)
    {
        item = newItem;
        count = newCount;

        icon.sprite = item.icon;
        icon.enabled = true;

        countText.text = count.ToString();
    }

    public void AddCount(int value)
    {
        count += value;
        countText.text = count.ToString();
    }

    public void ClearSlot()
    {
        item = null;
        count = 0;

        icon.enabled = false;
        countText.text = "";
    }

    public Item GetItem()
    {
        return item;
    }

    // 우클릭 감지 코드
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (item == null) return;

            RightClickMenu.instance.OpenMenu(this, Input.mousePosition);
        }
    }
}
