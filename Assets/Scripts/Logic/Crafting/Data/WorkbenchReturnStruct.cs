/// <summary>
/// 제작대에서 사용할 구조체 입니다.
/// 레시피의 요구 조건과 인벤토리의 상황을 비교하여 반환하는데 사용됩니다.
/// </summary>
public class WorkbenchReturnStruct
{
    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private int _curHasCount;
    private int _requireCount;
    private bool _isCondition;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public int CurHasCount=>_curHasCount;
    public int RequireCount=>_requireCount;
    public bool IsCondition => _isCondition;
    
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    public WorkbenchReturnStruct(int curHasCount, int requireCount,  bool isCondition)
    {
        _curHasCount = curHasCount;
        _requireCount = requireCount;
        _isCondition = isCondition;
    }
    #endregion
}
