using UnityEngine;

/// <summary>
/// 플레이어 도구 퀵슬롯 계산 전용 유틸리티
/// 1. 퀵슬롯 5칸의 고정 타입 순서를 관리한다.
/// 2. 인벤토리에서 각 타입별 최고 등급 도구를 찾는다.
/// 3. 같은 등급이면 레벨, 그 다음 강도, 그 다음 앞 슬롯 우선으로 비교한다.
/// </summary>
public static class ToolQuickSlotUtility
{
    public const int QUICK_SLOT_COUNT = 5;

    private static readonly EType[] s_quickSlotToolTypes =
    {
        EType.WateringCan, // 0
        EType.ShovelItem,  // 1
        EType.PickaxeItem, // 2
        EType.SickleItem,  // 3
        EType.Fishingrod   // 4
    };

    public struct QuickSlotToolData
    {
        public int quickSlotIndex;
        public EType toolType;
        public int inventorySlotIndex;
        public string itemId;
        public ToolItemSO itemSO;

        public bool HasItem => itemSO != null && string.IsNullOrWhiteSpace(itemId) == false;
        public Sprite Icon => itemSO != null ? itemSO.Image : null;
        public string Name => itemSO != null ? itemSO.Name : string.Empty;
        public ERarity Rarity => itemSO != null ? itemSO.Rarity : ERarity.None;

        public QuickSlotToolData(int quickSlotIndex, EType toolType, int inventorySlotIndex, string itemId, ToolItemSO itemSO)
        {
            this.quickSlotIndex = quickSlotIndex;
            this.toolType = toolType;
            this.inventorySlotIndex = inventorySlotIndex;
            this.itemId = itemId;
            this.itemSO = itemSO;
        }

        public static QuickSlotToolData CreateEmpty(int quickSlotIndex, EType toolType)
        {
            return new QuickSlotToolData(quickSlotIndex, toolType, -1, string.Empty, null);
        }
    }

    public static bool IsToolQuickSlotIndex(int quickSlotIndex)
    {
        return quickSlotIndex >= 0 && quickSlotIndex < QUICK_SLOT_COUNT;
    }

    public static EType GetToolTypeByQuickSlotIndex(int quickSlotIndex)
    {
        if (IsToolQuickSlotIndex(quickSlotIndex) == false)
        {
            return EType.None;
        }

        return s_quickSlotToolTypes[quickSlotIndex];
    }

    public static int GetQuickSlotIndexByToolType(EType toolType)
    {
        for (int i = 0; i < s_quickSlotToolTypes.Length; i++)
        {
            if (s_quickSlotToolTypes[i] == toolType)
            {
                return i;
            }
        }

        return -1;
    }

    public static QuickSlotToolData[] BuildQuickSlotData(Inventory inventory)
    {
        QuickSlotToolData[] results = new QuickSlotToolData[QUICK_SLOT_COUNT];

        for (int i = 0; i < QUICK_SLOT_COUNT; i++)
        {
            results[i] = QuickSlotToolData.CreateEmpty(i, s_quickSlotToolTypes[i]);
        }

        if (inventory == null || inventory.InventorySlots == null)
        {
            return results;
        }

        InventorySlot[] slots = inventory.InventorySlots;
        int slotCount = slots.Length;

        for (int i = 0; i < slotCount; i++)
        {
            InventorySlot slot = slots[i];

            if (slot.IsEmpty)
            {
                continue;
            }

            ToolItemSO toolItemSO = slot.ItemSO as ToolItemSO;
            if (toolItemSO == null)
            {
                continue;
            }

            int quickSlotIndex = GetQuickSlotIndexByToolType(toolItemSO.Type);
            if (quickSlotIndex < 0)
            {
                continue;
            }

            QuickSlotToolData candidate = new QuickSlotToolData(
                quickSlotIndex,
                toolItemSO.Type,
                i,
                toolItemSO.Id,
                toolItemSO);

            if (results[quickSlotIndex].HasItem == false || IsBetterCandidate(candidate, results[quickSlotIndex]))
            {
                results[quickSlotIndex] = candidate;
            }
        }

        return results;
    }

    public static bool TryGetQuickSlotTool(Inventory inventory, int quickSlotIndex, out QuickSlotToolData result)
    {
        result = QuickSlotToolData.CreateEmpty(quickSlotIndex, GetToolTypeByQuickSlotIndex(quickSlotIndex));

        if (IsToolQuickSlotIndex(quickSlotIndex) == false)
        {
            return false;
        }

        QuickSlotToolData[] all = BuildQuickSlotData(inventory);
        result = all[quickSlotIndex];
        return result.HasItem;
    }

    private static bool IsBetterCandidate(QuickSlotToolData candidate, QuickSlotToolData currentBest)
    {
        if (candidate.itemSO == null)
        {
            return false;
        }

        if (currentBest.itemSO == null)
        {
            return true;
        }

        if (candidate.itemSO.Rarity != currentBest.itemSO.Rarity)
        {
            return candidate.itemSO.Rarity > currentBest.itemSO.Rarity;
        }

        if (candidate.itemSO.CurrentLv != currentBest.itemSO.CurrentLv)
        {
            return candidate.itemSO.CurrentLv > currentBest.itemSO.CurrentLv;
        }

        if (Mathf.Approximately(candidate.itemSO.Strength, currentBest.itemSO.Strength) == false)
        {
            return candidate.itemSO.Strength > currentBest.itemSO.Strength;
        }

        return candidate.inventorySlotIndex < currentBest.inventorySlotIndex;
    }
}
