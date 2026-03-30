using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("UI")]
    public GameObject inventoryPanel;
    public Transform slotContainer;
    public GameObject slotPrefab;

    [Header("Settings")]
    public int totalSlots = 15;

    [Header("Test Items")]
    public List<ItemData> testItems;

    private List<InventorySlot> slots = new List<InventorySlot>();
    private bool isOpen = false;

    void Awake() => Instance = this;

    void Start()
    {
        InitSlots();
        AddTestItems();
        inventoryPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
            ToggleInventory();
    }

    void InitSlots()
    {
        for (int i = 0; i < totalSlots; i++)
        {
            GameObject go = Instantiate(slotPrefab, slotContainer);
            InventorySlot slot = go.GetComponent<InventorySlot>();
            slots.Add(slot);
        }
    }

    void AddTestItems()
    {
        foreach (var item in testItems)
            AddItem(item);
    }

    public bool AddItem(ItemData item)
    {
        foreach (var slot in slots)
        {
            if (slot.IsEmpty)
            {
                slot.SetItem(item);
                return true;
            }
        }
        Debug.Log("인벤토리가 가득 찼습니다!");
        return false;
    }

    public void RemoveItem(InventorySlot slot)
    {
        slot.ClearSlot();
    }

    public void ToggleInventory()
    {
        isOpen = !isOpen;
        inventoryPanel.SetActive(isOpen);
    }

    public void CloseInventory()
    {
        isOpen = false;
        inventoryPanel.SetActive(false);
    }
}
