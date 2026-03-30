using UnityEngine;

public class Inventory : MonoBehaviour
{
    public InventorySlot[] slots;

    public void AddItem(Item item)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].GetItem() == null)
            {
                slots[i].SetItem(item, 1);
                return;
            }
        }

        Debug.Log("인벤토리가 가득 찼습니다!");
    }
}
