using UnityEngine;

public class ItemPickupTest : MonoBehaviour
{
    public Inventory inventory;
    public Item testItem;

    public void AddItemButton()
    {
        inventory.AddItem(testItem);
    }
}
