using UnityEngine;

/// <summary>
/// 제작대에서 사용할 구조체 입니다.
/// 레시피의 요구 조건과 인벤토리의 상황을 비교하여 반환하는데 사용됩니다.
/// </summary>
public class WorkbenchReturnStruct
{
    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private Sprite _iconImg;
    private int _curHasCount;
    private int _requireCount;
    private bool _isCondition;
    private string _itemName;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public Sprite Icon => _iconImg;
    public int CurHasCount=>_curHasCount;
    public int RequireCount=>_requireCount;
    public bool IsCondition => _isCondition;
    public string ItemName => _itemName;
    #endregion

    #region 테스트용
    private int _slotIdx;
    public int SlotIdx => _slotIdx;
    #endregion


    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    public WorkbenchReturnStruct(Sprite icon, string itemName, int curHasCount, int requireCount, int slotIdx, bool isCondition)
    {
        _iconImg = icon;
        _itemName = itemName;
        _curHasCount = curHasCount;
        _requireCount = requireCount;
        _isCondition = isCondition;
        _slotIdx = slotIdx;
    }
    #endregion
}
