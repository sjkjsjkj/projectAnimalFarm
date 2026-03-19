using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 콘텐츠 파트 검증용 임시 인벤토리
/// 실제 팀 인벤토리 시스템이 들어오면 교체하거나 연결용으로 활용
/// </summary>
public class CContentTestInventory : MonoBehaviour, IItemReceiver
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [SerializeField] private bool _logOnAdd = true;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private readonly Dictionary<EItem, int> _itemTable = new Dictionary<EItem, int>();
    #endregion

    #region ─────────────────────────▶ 외부 메서드 ◀─────────────────────────
    public bool TryAddItem(EItem itemId, int amount)
    {
        if (itemId == EItem.None)
        {
            Debug.LogWarning("None 아이템은 추가할 수 없습니다.");
            return false;
        }

        if (amount <= 0)
        {
            Debug.LogWarning($"잘못된 수량입니다. item={itemId}, amount={amount}");
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

        if (_logOnAdd)
        {
            Debug.Log($"[인벤토리] {itemId} +{amount} / 현재 보유량: {_itemTable[itemId]}");
        }

        return true;
    }

    public int GetAmount(EItem itemId)
    {
        if (_itemTable.TryGetValue(itemId, out int amount))
        {
            return amount;
        }

        return 0;
    }
    #endregion
}
