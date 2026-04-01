using System;

/// <summary>
/// 인벤토리 한 칸이 가지는 런타임 데이터입니다.
/// ItemSO 참조와 현재 수량을 보관합니다.
/// </summary>
[Serializable]
public struct InventorySlotData
{
    #region ─────────────────────────▶ 접근자 ◀─────────────────────────
    public ItemSO Item => _item;
    public int Count => _count;
    public bool IsEmpty => _item == null || _count <= 0;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private ItemSO _item;
    private int _count;
    #endregion

    #region ─────────────────────────▶ 외부 메서드 ◀─────────────────────────
    /// <summary>
    /// 슬롯 데이터를 생성합니다.
    /// </summary>
    public InventorySlotData(ItemSO item, int count)
    {
        _item = item;
        _count = count;
    }

    /// <summary>
    /// 빈 슬롯 데이터를 반환합니다.
    /// </summary>
    public static InventorySlotData CreateEmpty()
    {
        return new InventorySlotData(null, 0);
    }
    #endregion
}
