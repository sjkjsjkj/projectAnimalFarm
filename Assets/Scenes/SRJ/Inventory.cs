using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public List<InventorySlot> slots = new List<InventorySlot>();

    public void AddItem(Item item)
    {
        // 1. 같은 아이템이 있는지 먼저 확인
        foreach (InventorySlot slot in slots)
        {
            if (!slot.IsEmpty() && slot.GetItem() == item)
            {
                slot.AddCount(1);
                return;
            }
        }

        // 2. 빈 슬롯 찾기
        foreach (InventorySlot slot in slots)
        {
            if (slot.IsEmpty())
            {
                slot.SetItem(item, 1);
                return;
            }
        }

        Debug.Log("인벤토리가 가득 찼습니다!");
    }
}
