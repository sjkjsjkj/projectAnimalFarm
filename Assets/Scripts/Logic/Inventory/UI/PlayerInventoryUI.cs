using UnityEngine;

/// <summary>
/// 플레이어 인벤토리의 UI 입니다.
/// </summary>
public class PlayerInventoryUI : InventoryUI
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [SerializeField] private RectTransform _contentsArea;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────

    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────

    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    public override void SetInfo(int invenSize)
    {
        UDebug.Print("InventoryUI SetInfo");
        for (int i = 0; i < invenSize; i++)
        {
            GameObject tempSlotUI = Instantiate(_inventorySlotUIPrefab);
            tempSlotUI.GetComponent<PlayerInventorySlotUI>().SetInfo();
            tempSlotUI.transform.SetParent(_contentsArea);
        }
    }
    public override void ShowUI()
    {
        throw new System.NotImplementedException();
    }
    public override void RefreshInventoryUI()
    {
        throw new System.NotImplementedException();
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    private void Awake()
    {
        
    }
    #endregion

    #region ─────────────────────────▶ 중첩 타입 ◀─────────────────────────

    #endregion
}
