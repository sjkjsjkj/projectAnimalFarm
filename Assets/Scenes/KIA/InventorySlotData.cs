using System;
using UnityEngine;

/// <summary>
/// 인벤토리 슬롯 UI 1칸을 그리기 위한 표시 전용 데이터입니다.
/// 실제 아이템 원본 데이터는 외부 인벤토리 시스템이 관리하고,
/// UI에는 아이콘과 수량만 전달합니다.
/// </summary>
[Serializable]
public struct InventorySlotData
{
    #region ─────────────────────────▶ 접근자 ◀─────────────────────────
    public Sprite Icon => _icon;
    public int Count => _count;
    public bool IsEmpty => _icon == null || _count <= 0;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    [SerializeField] private Sprite _icon;
    [SerializeField] private int _count;
    #endregion

    #region ─────────────────────────▶ 외부 메서드 ◀─────────────────────────
    /// <summary>
    /// 슬롯 표시 데이터를 생성합니다.
    /// </summary>
    public InventorySlotData(Sprite icon, int count)
    {
        _icon = icon;
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
