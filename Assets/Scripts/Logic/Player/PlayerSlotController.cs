using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어 슬롯과 손에 든 아이템을 관리하는 컴포넌트
/// </summary>
public class PlayerSlotController : BaseMono, IStatusLogical
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("손에 든 아이템 참조 연결")]
    [SerializeField] private Transform _heldItemTr;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    // 인벤토리에서 타입이 일치하는 도구 모두 반환
    public string[] GetTools(EType toolType)
    {
        if (TryGetComponent(out Inventory inventory)) return null;
        // 모두 수집
        List<string> tools = new();
        InventorySlot[] slots = inventory.InventorySlots;
        int length = slots.Length;
        for (int i = 0; i < length; ++i)
        {
            var slot = slots[i];
            if (slot.IsEmpty) continue;
            if (slot.ItemSO.Type != toolType) continue;
            // 일치하는 아이템
            tools.Add(slot.ItemSO.Name);
        }
        // 반환
        return tools.ToArray();
    }

    // 손에 특정 아이템을 들도록 변경 시도
    public bool TrySwapTool(string toolId, out string message)
    {
        message = string.Empty;
        if (!TryGetPlayerInventory(out Inventory inventory)) return false;
        // 인벤토리 전체에서 아이템 탐색 시도
        int targetSlotIndex = -1;
        int length = inventory.InventorySlots.Length;
        for (int i = 0; i < length; i++)
        {
            var slot = inventory.InventorySlots[i];
            if (!slot.IsEmpty && slot.ItemSO.Id == toolId)
            {
                targetSlotIndex = i;
                break;
            }
        }
        // 인벤토리 전체에서 탐색 실패
        if (targetSlotIndex == -1)
        {
            message = "인벤토리에서 해당 도구를 찾을 수 없습니다.";
            return false;
        }
        // 찾은 슬롯 번호로 장착 실행
        EquipItem(targetSlotIndex, toolId);
        message = $"{toolId}를 손에 들었습니다.";
        return true;
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    // 플레이어가 슬롯을 변경했을 경우
    private void PlayerSelectSlotHandle(OnPlayerSelectSlot ctx)
    {
        if (!TryGetPlayerInventory(out Inventory inventory)) return;
        // 변수 준비
        var slots = inventory.InventorySlots;
        int safeIndex = Mathf.Clamp(ctx.slot, 0, slots.Length - 1);
        InventorySlot selectSlot = slots[ctx.slot];
        string itemId = selectSlot.IsEmpty ? string.Empty : selectSlot.ItemSO.Id;
        EquipItem(safeIndex, itemId);
    }

    // 아이템을 장비한다. (모델링 교체)
    private void EquipItem(int slot, string itemId)
    {
        if (!TryGetPlayerProvider(out PlayerProvider player)) return;
        // 손에 든 아이템을 다른 아이템으로 변경 시도
        if(player.TrySetSlotIndex(slot, itemId))
        {
            // 그래픽 갱신
            
        }
    }

    // 플레이어 인벤토리의 무결성 검사 및 가져오기
    private bool TryGetPlayerInventory(out Inventory inventory)
    {
        var manager = InventoryManager.Ins;
        inventory = null;
        if (InventoryManager.Ins == null)
        {
            UDebug.Print($"인벤토리 매니저가 존재하지 않습니다.", LogType.Assert);
            return false;
        }
        inventory = InventoryManager.Ins.PlayerInventory;
        if (inventory == null)
        {
            UDebug.Print($"플레이어 인벤토리가 존재하지 않습니다.", LogType.Error);
            return false;
        }
        var slots = inventory.InventorySlots;
        if (slots == null)
        {
            UDebug.Print($"플레이어 인벤토리 슬롯 배열이 존재하지 않습니다.", LogType.Error);
            return false;
        }
        if (slots.Length <= 0)
        {
            UDebug.Print($"플레이어 인벤토리 슬롯 배열의 길이가 0입니다.", LogType.Error);
            return false;
        }
        return true;
    }

    // 플레이어 프로바이더의 무결성 검사 및 가져오기
    private bool TryGetPlayerProvider(out PlayerProvider provider)
    {
        var manager = DataManager.Ins;
        provider = null;
        if (DataManager.Ins == null)
        {
            UDebug.Print($"데이터 매니저가 존재하지 않습니다.", LogType.Assert);
            return false;
        }
        provider = DataManager.Ins.Player;
        if (provider == null)
        {
            UDebug.Print($"플레이어 인벤토리가 존재하지 않습니다.", LogType.Error);
            return false;
        }
        if (provider.IsEmpty())
        {
            UDebug.Print($"플레이어 데이터가 초기화되지 않았습니다.", LogType.Error);
            return false;
        }
        return true;
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    private void OnEnable()
    {
        EventBus<OnPlayerSelectSlot>.Subscribe(PlayerSelectSlotHandle);
    }
    private void OnDisable()
    {
        EventBus<OnPlayerSelectSlot>.Unsubscribe(PlayerSelectSlotHandle);
    }
    #endregion
}
