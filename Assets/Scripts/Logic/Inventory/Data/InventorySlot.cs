/// <summary>
/// 인벤토리의 각각의 슬롯의 속성들을 가진 구조체 입니다.
/// </summary>
public class InventorySlot
{
    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private ItemSO _itemData;
    private int _stack;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public bool IsEmpty => _itemData != null;
    public string SlotItemId => _itemData.Id;
    public int CurStack => _stack;
    #endregion

    #region ─────────────────────────▶ 생성자 ◀─────────────────────────
    public InventorySlot()
    {
        _itemData = null;
        _stack = 0;
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    /// <summary>
    /// 아이템 획득
    /// </summary>

    public void GetItem(ItemSO itemData)
    {
        if(itemData == null) return;

        if(_itemData == null)
        {
            _itemData = itemData;
            _stack = 1;
        }
        else
        {
            if( _itemData.Id.CompareTo(itemData.Id) != 0 )
            {
                UDebug.Print("슬롯에 들어있던 아이디와 획득한 아이템의 아이디가 다름.", UnityEngine.LogType.Warning);
            }
            _stack++;
        }
    }
    /// <summary>
    /// 해당 슬롯의 아이템 버리기
    /// </summary>
    public void ItemDraw()
    {
        if(_itemData == null) return;

        _itemData = null;
        _stack = 0;
    }
    #endregion
}
