using UnityEngine;

public class RightClickMenu : MonoBehaviour
{
    public static RightClickMenu instance;

    InventorySlot currentSlot;

    void Awake()
    {
        instance = this;
        gameObject.SetActive(false);
    }

    public void OpenMenu(InventorySlot slot, Vector2 position)
    {
        currentSlot = slot;
        gameObject.SetActive(true);
        transform.position = position;
    }

    public void UseItem()
    {
        if (currentSlot != null)
        {
            Debug.Log("아이템 사용!");
            currentSlot.ClearSlot();
        }

        CloseMenu();
    }

    public void DropItem()
    {
        if (currentSlot != null)
        {
            Debug.Log("아이템 버리기!");
            currentSlot.ClearSlot();
        }

        CloseMenu();
    }

    public void Cancel()
    {
        CloseMenu();
    }

    void CloseMenu()
    {
        gameObject.SetActive(false);
    }
}
