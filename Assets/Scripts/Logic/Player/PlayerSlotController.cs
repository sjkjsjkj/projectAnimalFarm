using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어 슬롯과 손에 든 아이템을 관리하는 컴포넌트
/// 수정 내용
/// 1. 0~4번 슬롯은 고정 도구 퀵슬롯으로 사용
///    - 0 : 물뿌리개
///    - 1 : 삽
///    - 2 : 곡괭이
///    - 3 : 낫
///    - 4 : 낚싯대
/// 2. 각 도구 퀵슬롯은 플레이어 인벤토리에서 가장 등급이 높은 도구를 자동 장착
/// 3. 5~9번 슬롯은 기존처럼 인벤토리 실제 슬롯 선택을 유지
/// 4. 상태창에서 도구 목록을 요청할 때 itemId 기준으로 반환하도록 수정
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
        if (!TryGetComponent(out Inventory inventory)) return null;
        // 모두 수집
        List<string> tools = new();
        InventorySlot[] slots = inventory.InventorySlots;
        int length = slots.Length;
        for (int i = 0; i < length; ++i)
        {
            InventorySlot slot = slots[i];
            if (slot.IsEmpty) continue;
            if (slot.ItemSO == null || slot.ItemSO.Type != toolType)
            {
                continue;
            }
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
        if (!TryGetPlayerInventory(out Inventory inventory))
            {
            message = "플레이어 인벤토리를 찾을 수 없습니다.";
            return false;
            }
        // 인벤토리 전체에서 아이템 탐색 시도
        int targetSlotIndex = -1;
        int length = inventory.InventorySlots.Length;
        for (int i = 0; i < length; i++)
        {
            InventorySlot slot = inventory.InventorySlots[i];
            if (!slot.IsEmpty && slot.ItemSO != null && slot.ItemSO.Id == toolId)
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

        int requestedSlot = Mathf.Clamp(ctx.slot, 0, 9);

        // 고정 도구 퀵슬롯(0~4)
        if (ToolQuickSlotUtility.IsToolQuickSlotIndex(requestedSlot))
        {
            if (ToolQuickSlotUtility.TryGetQuickSlotTool(inventory, requestedSlot, out ToolQuickSlotUtility.QuickSlotToolData quickSlotTool)
                && quickSlotTool.HasItem)
            {
                EquipItem(requestedSlot, quickSlotTool.itemId);
            }
            else
            {
                // 해당 타입 도구가 하나도 없으면 손에 든 아이템 비우기
                EquipItem(requestedSlot, string.Empty);
            }

            return;
        }

        // 기존 실제 인벤토리 슬롯 선택(5~9)
        InventorySlot[] slots = inventory.InventorySlots;
        if (slots == null || slots.Length <= 0)
        {
            return;
        }

        int safeIndex = Mathf.Clamp(requestedSlot, 0, slots.Length - 1);
        InventorySlot selectSlot = slots[safeIndex];
        string itemId = (selectSlot.IsEmpty || selectSlot.ItemSO == null) ? string.Empty : selectSlot.ItemSO.Id;

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
        inventory = null;
        if (InventoryManager.Ins == null)
        {
            UDebug.Print($"인벤토리 매니저가 존재하지 않습니다.", LogType.Assert);
            return false;
        }

        if (InventoryManager.Ins.IsSettingFinish == false)
        {
            UDebug.Print("인벤토리 매니저 세팅이 아직 끝나지 않았습니다.", LogType.Warning);
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
