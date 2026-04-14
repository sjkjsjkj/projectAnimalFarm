using System;
using UnityEngine;

/// <summary>
/// 플레이어 퀵슬롯(5칸)에 등록된 도구를 기준으로
/// 사용 가능 여부를 검사하는 유틸리티
/// </summary>
public static class PlayerQuickSlotToolUtility
{
    public static bool TryGetQuickSlotTool(EType toolType, out ToolItemSO toolItemSO)
    {
        toolItemSO = null;

        if (toolType == EType.None)
        {
            return false;
        }

        if (InventoryManager.Ins == null || InventoryManager.Ins.PlayerInventory == null)
        {
            return false;
        }

        Inventory inventory = InventoryManager.Ins.PlayerInventory;
        int quickSlotIndex = ToolQuickSlotUtility.GetQuickSlotIndexByToolType(toolType);

        if (quickSlotIndex < 0)
        {
            return false;
        }

        ToolQuickSlotUtility.QuickSlotToolData slotData;
        bool hasTool = ToolQuickSlotUtility.TryGetQuickSlotTool(inventory, quickSlotIndex, out slotData);

        if (!hasTool || !slotData.HasItem || slotData.itemSO == null)
        {
            return false;
        }

        toolItemSO = slotData.itemSO;
        return true;
    }

    public static bool HasRequiredQuickSlotTool(
        EType requiredToolType,
        ERarity minRarity,
        out ToolItemSO quickSlotTool,
        out string failMessage)
    {
        quickSlotTool = null;
        failMessage = string.Empty;

        if (requiredToolType == EType.None)
        {
            return true;
        }

        if (!TryGetQuickSlotTool(requiredToolType, out quickSlotTool))
        {
            failMessage = $"퀵슬롯에 {GetToolTypeDisplayName(requiredToolType)}가 있어야 합니다.";
            return false;
        }

        ERarity normalizedMinRarity = NormalizeMinRarity(minRarity);

        if (quickSlotTool.Rarity < normalizedMinRarity)
        {
            failMessage = $"퀵슬롯에 {GetRarityDisplayName(normalizedMinRarity)} 등급 이상의 {GetToolTypeDisplayName(requiredToolType)}가 있어야 합니다.";
            return false;
        }

        return true;
    }

    public static bool TryParseRarityText(string rawText, out ERarity rarity)
    {
        rarity = ERarity.None;

        if (string.IsNullOrWhiteSpace(rawText))
        {
            return false;
        }

        string key = rawText.Trim().ToLowerInvariant();

        switch (key)
        {
            case "basic":
            case "기본":
                rarity = ERarity.Basic;
                return true;

            case "solid":
            case "견고":
                rarity = ERarity.Solid;
                return true;

            case "superior":
            case "rare":
            case "고급":
            case "희귀":
                rarity = ERarity.Superior;
                return true;

            case "prime":
            case "프라임":
                rarity = ERarity.Prime;
                return true;

            case "masterwork":
            case "master":
            case "legend":
            case "legendary":
            case "마스터":
            case "전설":
                rarity = ERarity.Masterwork;
                return true;
        }

        return Enum.TryParse(rawText, true, out rarity) && rarity != ERarity.None;
    }

    public static string GetToolTypeDisplayName(EType toolType)
    {
        switch (toolType)
        {
            case EType.SickleItem:
                return "낫";

            case EType.ShovelItem:
                return "삽";

            case EType.AxeItem:
                return "도끼";

            case EType.PickaxeItem:
                return "곡괭이";

            case EType.WateringCan:
                return "물뿌리개";

            case EType.Fishingrod:
                return "낚싯대";

            default:
                return "도구";
        }
    }

    public static string GetRarityDisplayName(ERarity rarity)
    {
        switch (rarity)
        {
            case ERarity.Basic:
                return "기본";

            case ERarity.Solid:
                return "견고";

            case ERarity.Superior:
                return "고급";

            case ERarity.Prime:
                return "프라임";

            case ERarity.Masterwork:
                return "마스터워크";

            default:
                return "기본";
        }
    }

    private static ERarity NormalizeMinRarity(ERarity rarity)
    {
        return rarity == ERarity.None ? ERarity.Basic : rarity;
    }
}
