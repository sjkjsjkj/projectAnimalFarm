using UnityEngine;

/// <summary>
/// 상점의 UI 입니다.
/// </summary>
public class ShopUI : InventoryUI
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [SerializeField] private GameObject _itemInfoFiledPrefab;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private GameObject[] _itemInfoFileds;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────

    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    public override void SetInfo(int invenSize)
    {
        for(int i=0; i< _inventorySize; i++)
        {
            _itemInfoFileds[i] = Instantiate(_itemInfoFiledPrefab);
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

    #endregion

    #region ─────────────────────────▶ 중첩 타입 ◀─────────────────────────

    #endregion
}
