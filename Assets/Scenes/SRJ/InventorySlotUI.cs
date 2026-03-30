using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI countText;

    public void SetSlot(ItemData item, int count)
    {
        icon.sprite = item.icon;
        icon.color = Color.white;

        countText.text = count.ToString();
    }

    public void ClearSlot()
    {
        icon.sprite = null;
        icon.color = new Color(1, 1, 1, 0);

        countText.text = "";
    }
}
