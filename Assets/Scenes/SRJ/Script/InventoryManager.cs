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
        {
            AddItem(item);
            AddItem(item); // 스택 테스트용 2개씩 추가
        }
    }

    public bool AddItem(ItemData item, int count = 1)
    {
        // 같은 아이템 있으면 스택
        foreach (var slot in slots)
        {
            if (!slot.IsEmpty && slot.CurrentItem == item && slot.ItemCount < item.maxStack)
            {
                slot.AddCount(count);
                return true;
            }
        }

        // 빈 슬롯에 추가
        foreach (var slot in slots)
        {
            if (slot.IsEmpty)
            {
                slot.SetItem(item, count);
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
