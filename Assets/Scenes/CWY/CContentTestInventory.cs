using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 시트 기반 문자열 ID를 사용하는 인벤토리
/// 구글시트의 ID(예: Apple_0, River_1)를 그대로 키로 사용한다.
/// </summary>
public class CContentTestInventory : BaseMono
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [SerializeField] private bool _logOnAdd = true;
    [SerializeField] private SheetItemDatabase _database;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    // itemId -> amount
    private readonly Dictionary<string, int> _itemTable = new Dictionary<string, int>();
    #endregion

    #region ─────────────────────────▶ 외부 메서드 ◀─────────────────────────
    /// <summary>
    /// 문자열 ID 기반으로 아이템을 추가한다.
    /// </summary>
    public bool TryAddItem(string itemId, int amount)
    {
        if (string.IsNullOrWhiteSpace(itemId))
        {
            Debug.LogWarning("[Inventory] itemId가 비어 있습니다.");
            return false;
        }

        if (amount <= 0)
        {
            Debug.LogWarning($"[Inventory] 잘못된 수량입니다. itemId={itemId}, amount={amount}");
            return false;
        }

        // DB에 없는 아이템이면 잘못된 데이터일 가능성이 높음
        if (_database != null && !_database.TryGetRow(itemId, out SheetItemRow row))
        {
            Debug.LogWarning($"[Inventory] DB에 존재하지 않는 itemId입니다: {itemId}");
            return false;
        }

        if (_itemTable.ContainsKey(itemId))
        {
            _itemTable[itemId] += amount;
        }
        else
        {
            _itemTable.Add(itemId, amount);
        }

        // 이벤트 발행
        OnInventoryItemChanged.Publish(
            slot: -1,                 // 아직 슬롯 고정 구조가 아니므로 -1
            id: itemId,
            amount: _itemTable[itemId]
        );

        if (_logOnAdd)
        {
            string itemName = itemId;

            if (_database != null && _database.TryGetRow(itemId, out SheetItemRow itemRow))
            {
                itemName = itemRow.name;
            }

            Debug.Log($"[Inventory] {itemName}({itemId}) +{amount} / 현재 보유량: {_itemTable[itemId]}");
        }

        return true;
    }

    /// <summary>
    /// 특정 아이템 현재 수량 반환
    /// </summary>
    public int GetAmount(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
        {
            return 0;
        }

        if (_itemTable.TryGetValue(itemId, out int amount))
        {
            return amount;
        }

        return 0;
    }

    /// <summary>
    /// 가지고 있는지 여부
    /// </summary>
    public bool HasItem(string itemId)
    {
        return GetAmount(itemId) > 0;
    }

    /// <summary>
    /// 인벤토리 전체 데이터 반환
    /// UI에서 순회할 때 사용
    /// </summary>
    public Dictionary<string, int> GetAllItems()
    {
        return new Dictionary<string, int>(_itemTable);
    }
    #endregion
}
