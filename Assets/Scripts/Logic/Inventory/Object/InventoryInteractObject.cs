/// <summary>
/// 플레이어가 인벤토리를 인터랙션 할수 있게 매개체.
/// </summary>
public class InventoryInteractObject : BaseMono
{
    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private int _idx;
    private EInventoryType _invenType;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────

    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    public void SetInfo(int idx, EInventoryType invenType)
    {
        _idx = idx;
        _invenType = invenType;
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    public void InventoryOpenRequest()
    {
        //inventoryM
    }
    #endregion

    #region ─────────────────────────▶ 중첩 타입 ◀─────────────────────────

    #endregion
}
