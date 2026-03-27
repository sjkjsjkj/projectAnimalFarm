using System.Collections.Generic;
using UnityEngine;

public class StorageManager : MonoBehaviour
{
    public List<InventorySlot> slots = new List<InventorySlot>();

    public bool AddItem(Item item)
    {
        foreach (InventorySlot slot in slots)
        {
            if (slot.IsEmpty())
            {
                slot.SetItem(item, 1);
                return true;
            }
        }

        Debug.Log("창고가 가득 찼습니다!");
        return false;
    }
}
