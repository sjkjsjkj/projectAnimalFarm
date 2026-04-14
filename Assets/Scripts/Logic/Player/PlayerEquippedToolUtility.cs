using System;
using UnityEngine;

/// <summary>
/// 플레이어가 현재 손에 든 도구 정보를 공통으로 읽는 유틸리티
/// 1. 현재 장착한 도구를 가져온다.
/// 2. 인벤토리에서 특정 타입의 최고 도구를 찾는다.
/// 3. 장착 도구의 타입/등급이 조건을 만족하는지 검사한다.
/// 4. 문자열 등급(Fish TSV 등) -> ERarity 변환을 보조한다.
/// </summary>
public static class PlayerEquippedToolUtility
{
    public static bool TryGetHeldToolItemSO(out ToolItemSO toolItemSO)
    {
        toolItemSO = null;

        if (DataManager.Ins == null || DataManager.Ins.Player == null)
        {
            return false;
        }

        string heldItemId = DataManager.Ins.Player.HeldItemId;
        if (string.IsNullOrWhiteSpace(heldItemId))
        {
            return false;
        }

        return TryGetToolItemSO(heldItemId, out toolItemSO);
    }

    public static bool TryGetToolItemSO(string itemId, out ToolItemSO toolItemSO)
    {
        toolItemSO = null;

        if (string.IsNullOrWhiteSpace(itemId))
        {
            return false;
        }

        if (DatabaseManager.Ins == null)
        {
            return false;
        }

        ItemSO itemSO = DatabaseManager.Ins.Item(itemId);
        toolItemSO = itemSO as ToolItemSO;
        return toolItemSO != null;
    }

    public static bool HasRequiredEquippedTool(
        EType requiredToolType,
        ERarity minRarity,
        out ToolItemSO equippedTool,
        out string failMessage)
    {
        equippedTool = null;
        failMessage = string.Empty;

        if (requiredToolType == EType.None)
        {
            return true;
        }

        if (!TryGetHeldToolItemSO(out equippedTool))
        {
            failMessage = $"{GetToolTypeDisplayName(requiredToolType)}를 장착해야 합니다.";
            return false;
        }

        if (equippedTool.Type != requiredToolType)
        {
            failMessage = $"{GetToolTypeDisplayName(requiredToolType)}를 장착해야 합니다.";
            return false;
        }

        ERarity normalizedMinRarity = NormalizeMinRarity(minRarity);

        if (equippedTool.Rarity < normalizedMinRarity)
        {
            failMessage = $"{GetRarityDisplayName(normalizedMinRarity)} 등급 이상의 {GetToolTypeDisplayName(requiredToolType)}가 필요합니다.";
            return false;
        }

        return true;
    }

    public static bool TryGetBestToolInInventory(Inventory inventory, EType toolType, out ToolItemSO bestTool)
    {
        bestTool = null;

        if (inventory == null || inventory.InventorySlots == null)
        {
            return false;
        }

        InventorySlot[] slots = inventory.InventorySlots;

        for (int i = 0; i < slots.Length; i++)
        {
            InventorySlot slot = slots[i];

            if (slot.IsEmpty)
            {
                continue;
            }

            if (slot.ItemSO == null)
            {
                continue;
            }

            ToolItemSO candidate = slot.ItemSO as ToolItemSO;
            if (candidate == null)
            {
                continue;
            }

            if (candidate.Type != toolType)
            {
                continue;
            }

            if (bestTool == null || IsBetterTool(candidate, bestTool))
            {
                bestTool = candidate;
            }
        }

        return bestTool != null;
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

    private static bool IsBetterTool(ToolItemSO candidate, ToolItemSO currentBest)
    {
        if (candidate == null)
        {
            return false;
        }

        if (currentBest == null)
        {
            return true;
        }

        if (candidate.Rarity != currentBest.Rarity)
        {
            return candidate.Rarity > currentBest.Rarity;
        }

        if (candidate.CurrentLv != currentBest.CurrentLv)
        {
            return candidate.CurrentLv > currentBest.CurrentLv;
        }

        if (!Mathf.Approximately(candidate.Strength, currentBest.Strength))
        {
            return candidate.Strength > currentBest.Strength;
        }

        return string.Compare(candidate.Id, currentBest.Id, StringComparison.Ordinal) < 0;
    }
}
