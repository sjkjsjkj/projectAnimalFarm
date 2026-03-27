using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public List<InventorySlot> slots = new List<InventorySlot>();

    public bool AddItem(Item item)
    {
        // 같은 아이템 먼저 찾기
        foreach (InventorySlot slot in slots)
        {
            if (!slot.IsEmpty() && slot.GetItem() == item)
            {
                slot.AddCount(1);
                return true;
            }
        }

        // 빈 슬롯 찾기
        foreach (InventorySlot slot in slots)
        {
            if (slot.IsEmpty())
            {
                slot.SetItem(item, 1);
                return true;
            }
        }

        Debug.Log("인벤토리가 가득 찼습니다!");
        return false;
    }
}
