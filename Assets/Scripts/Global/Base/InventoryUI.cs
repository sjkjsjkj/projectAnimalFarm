using UnityEngine;

/// <summary>
/// 인벤토리의 UI의 베이스 스크립트 입니다.
/// 플레이어의 인벤토리 / 창고 / 상점등이 이 베이스스크립트를 상속받아 사용할 예정입니다.
/// </summary>
public abstract class InventoryUI : BaseMono
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [SerializeField] protected GameObject _inventorySlotUIPrefab;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    protected int _inventorySize;
    protected InventorySlotUI[] _inventorySlotUIs;
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    public abstract void SetInfo(int invenSize);
    public void ShowUIRequest()
    {
        ShowUI();
    }
    public abstract void ShowUI();
    public abstract void RefreshInventoryUI();
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    
    #endregion

    #region ─────────────────────────▶ 중첩 타입 ◀─────────────────────────

    #endregion
}
