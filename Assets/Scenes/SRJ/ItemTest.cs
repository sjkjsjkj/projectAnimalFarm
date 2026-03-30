using UnityEngine;

public class ItemTest : MonoBehaviour
{
    public Item Item;
    public Inventory inventory;

    void Start()
    {
        inventory.AddItem(Item);
    }
}
