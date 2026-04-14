using UnityEngine;

/// <summary>
/// 낚시 미끼를 테스트용으로 인벤토리에 넣는 스크립트
///
/// 사용 방법
/// 1. 씬의 TestManager 같은 오브젝트에 붙인다.
/// 2. bait ItemSO를 직접 넣거나 bait itemId를 입력한다.
/// 3. ContextMenu로 인벤토리에 미끼를 지급한다.
/// </summary>
public class FishingBaitTestGiver : MonoBehaviour
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("미끼 선택")]
    [Tooltip("가장 추천하는 방식. 여기 미끼 ItemSO를 직접 넣으면 itemId를 몰라도 됩니다.")]
    [SerializeField] private ItemSO _baitItemSO;

    [Tooltip("ItemSO를 안 넣었을 때 사용할 미끼 itemId")]
    [SerializeField] private string _baitItemId = "";

    [Header("지급 수량")]
    [SerializeField] private int _amount = 1;

    [Header("지급 방식")]
    [Tooltip("체크하면 ItemCollectionCoordinator를 통해 지급합니다.")]
    [SerializeField] private bool _useCoordinator = true;

    [SerializeField] private ItemCollectionCoordinator _itemCoordinator;

    [Header("프리셋 미끼 목록")]
    [Tooltip("자주 테스트할 미끼들을 넣어두면 편합니다.")]
    [SerializeField] private ItemSO[] _baitPresets;

    [SerializeField] private int _selectedPresetIndex = 0;

    [Header("로그")]
    [SerializeField] private bool _logEnabled = true;
    #endregion

    #region ─────────────────────────▶ 공용 메서드 ◀─────────────────────────
    public bool TryGiveSelectedBait()
    {
        return TryGiveBait(_amount);
    }

    public bool TryGiveBait(int amount)
    {
        if (amount <= 0)
        {
            PrintLog($"[FishingBaitTestGiver] amount가 0 이하입니다. amount={amount}", LogType.Warning);
            return false;
        }

        if (!TryResolveTargetItem(out ItemSO targetItemSO, out string targetItemId))
        {
            return false;
        }

        if (_useCoordinator)
        {
            return TryGiveByCoordinator(targetItemId, amount);
        }

        return TryGiveDirect(targetItemSO, amount);
    }

    public bool TryGivePresetBait(int presetIndex, int amount)
    {
        if (!TryResolvePresetItem(presetIndex, out ItemSO presetItemSO, out string presetItemId))
        {
            return false;
        }

        if (amount <= 0)
        {
            PrintLog($"[FishingBaitTestGiver] amount가 0 이하입니다. amount={amount}", LogType.Warning);
            return false;
        }

        if (_useCoordinator)
        {
            return TryGiveByCoordinator(presetItemId, amount);
        }

        return TryGiveDirect(presetItemSO, amount);
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    private bool TryResolveTargetItem(out ItemSO itemSO, out string itemId)
    {
        itemSO = null;
        itemId = string.Empty;

        if (_baitItemSO != null)
        {
            itemSO = _baitItemSO;
            itemId = _baitItemSO.Id;

            if (string.IsNullOrWhiteSpace(itemId))
            {
                PrintLog("[FishingBaitTestGiver] _baitItemSO의 Id가 비어 있습니다.", LogType.Warning);
                return false;
            }

            return true;
        }

        if (!string.IsNullOrWhiteSpace(_baitItemId))
        {
            if (DatabaseManager.Ins == null)
            {
                PrintLog("[FishingBaitTestGiver] DatabaseManager.Ins가 null입니다.", LogType.Error);
                return false;
            }

            itemSO = DatabaseManager.Ins.Item(_baitItemId);
            if (itemSO == null)
            {
                PrintLog($"[FishingBaitTestGiver] itemId로 ItemSO를 찾지 못했습니다. itemId={_baitItemId}", LogType.Warning);
                return false;
            }

            itemId = _baitItemId;
            return true;
        }

        if (TryResolvePresetItem(_selectedPresetIndex, out ItemSO presetItemSO, out string presetItemId))
        {
            itemSO = presetItemSO;
            itemId = presetItemId;
            return true;
        }

        PrintLog("[FishingBaitTestGiver] 지급할 미끼가 설정되지 않았습니다. _baitItemSO 또는 _baitItemId 또는 _baitPresets를 확인하세요.", LogType.Warning);
        return false;
    }

    private bool TryResolvePresetItem(int presetIndex, out ItemSO itemSO, out string itemId)
    {
        itemSO = null;
        itemId = string.Empty;

        if (_baitPresets == null || _baitPresets.Length == 0)
        {
            PrintLog("[FishingBaitTestGiver] _baitPresets가 비어 있습니다.", LogType.Warning);
            return false;
        }

        if (presetIndex < 0 || presetIndex >= _baitPresets.Length)
        {
            PrintLog($"[FishingBaitTestGiver] presetIndex가 범위를 벗어났습니다. index={presetIndex}", LogType.Warning);
            return false;
        }

        itemSO = _baitPresets[presetIndex];
        if (itemSO == null)
        {
            PrintLog($"[FishingBaitTestGiver] 선택된 프리셋 미끼가 null입니다. index={presetIndex}", LogType.Warning);
            return false;
        }

        itemId = itemSO.Id;
        if (string.IsNullOrWhiteSpace(itemId))
        {
            PrintLog($"[FishingBaitTestGiver] 프리셋 미끼의 Id가 비어 있습니다. index={presetIndex}", LogType.Warning);
            return false;
        }

        return true;
    }

    private bool TryGiveByCoordinator(string itemId, int amount)
    {
        if (string.IsNullOrWhiteSpace(itemId))
        {
            PrintLog("[FishingBaitTestGiver] Coordinator 지급 실패: itemId가 비어 있습니다.", LogType.Warning);
            return false;
        }

        if (_itemCoordinator == null)
        {
            _itemCoordinator = ItemCollectionCoordinator.Ins;
        }

        if (_itemCoordinator == null)
        {
            PrintLog("[FishingBaitTestGiver] ItemCollectionCoordinator가 없습니다.", LogType.Error);
            return false;
        }

        bool result = _itemCoordinator.TryCollectItem(itemId, amount);

        PrintLog(
            result
                ? $"[FishingBaitTestGiver] Coordinator 경유 미끼 지급 성공: {itemId} x{amount}"
                : $"[FishingBaitTestGiver] Coordinator 경유 미끼 지급 실패: {itemId} x{amount}",
            result ? LogType.Log : LogType.Warning);

        return result;
    }

    private bool TryGiveDirect(ItemSO itemSO, int amount)
    {
        if (itemSO == null)
        {
            PrintLog("[FishingBaitTestGiver] 직접 지급 실패: itemSO가 null입니다.", LogType.Warning);
            return false;
        }

        if (InventoryManager.Ins == null)
        {
            PrintLog("[FishingBaitTestGiver] InventoryManager.Ins가 null입니다.", LogType.Error);
            return false;
        }

        if (InventoryManager.Ins.PlayerInventory == null)
        {
            PrintLog("[FishingBaitTestGiver] PlayerInventory가 null입니다.", LogType.Error);
            return false;
        }

        bool isAllSuccess = true;

        for (int i = 0; i < amount; i++)
        {
            bool result = InventoryManager.Ins.PlayerInventory.TryGetItem(itemSO);
            if (!result)
            {
                isAllSuccess = false;
                PrintLog($"[FishingBaitTestGiver] 직접 미끼 지급 실패: {itemSO.Id}, count={i + 1}/{amount}", LogType.Warning);
                break;
            }
        }

        if (isAllSuccess)
        {
            PrintLog($"[FishingBaitTestGiver] 직접 미끼 지급 성공: {itemSO.Id} x{amount}", LogType.Log);
        }

        return isAllSuccess;
    }

    private void PrintLog(string message, LogType logType)
    {
        if (!_logEnabled && logType == LogType.Log)
        {
            return;
        }

        switch (logType)
        {
            case LogType.Warning:
                Debug.LogWarning(message);
                break;

            case LogType.Error:
            case LogType.Assert:
            case LogType.Exception:
                Debug.LogError(message);
                break;

            default:
                Debug.Log(message);
                break;
        }
    }
    #endregion

    #region ─────────────────────────▶ 컨텍스트 메뉴 ◀─────────────────────────
    [ContextMenu("Inventory/Give Selected Bait")]
    private void ContextGiveSelectedBait()
    {
        TryGiveSelectedBait();
    }

    [ContextMenu("Inventory/Give Selected Bait x10")]
    private void ContextGiveSelectedBaitX10()
    {
        TryGiveBait(10);
    }

    [ContextMenu("Inventory/Give Selected Preset Bait")]
    private void ContextGiveSelectedPresetBait()
    {
        TryGivePresetBait(_selectedPresetIndex, _amount);
    }

    [ContextMenu("Inventory/Give All Preset Baits x1")]
    private void ContextGiveAllPresetBaits()
    {
        if (_baitPresets == null || _baitPresets.Length == 0)
        {
            PrintLog("[FishingBaitTestGiver] _baitPresets가 비어 있습니다.", LogType.Warning);
            return;
        }

        for (int i = 0; i < _baitPresets.Length; i++)
        {
            TryGivePresetBait(i, 1);
        }
    }
    #endregion
}
