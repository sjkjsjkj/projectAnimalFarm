using System;

/// <summary>
/// 인벤토리의 각각의 슬롯의 속성들을 가진 구조체 입니다.
/// </summary>
public struct InventorySlot
{
    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private ItemSO _itemData;
    private int _stack;
    private int _slotIdx;

    private string _sfxIdITemmoveSound;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public int SlotIdx => _slotIdx;
    public bool IsEmpty => _itemData == null;
    public string SlotItemId => _itemData.Id;
    public int CurStack => _stack;
    public ItemSO ItemSO => _itemData;

    #endregion

    #region ─────────────────────────▶ 생성자 ◀─────────────────────────
    public InventorySlot(int slotIdx)
    {
        _itemData = null;
        _stack = 0;
        _slotIdx = slotIdx;

        _sfxIdITemmoveSound = Id.Sfx_Ui_ItemMovement_3;

    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    /// <summary>
    /// 슬롯에 아이템 세팅
    /// </summary>
    public void SetItem(ItemSO itemData, int itemStack = 1)
    {
        if(itemData == null) return;

        if(_itemData == null)
        {
            _itemData = itemData;
            _stack = itemStack;
        }
        else
        {
            if( _itemData.Id.CompareTo(itemData.Id) != 0 )
            {
                UDebug.Print("슬롯에 들어있던 아이디와 획득한 아이템의 아이디가 다름.", UnityEngine.LogType.Warning);
            }
            _stack+= itemStack;
        }

        switch(itemData.Type)
        {
            case EType.PickaxeItem:
            case EType.SickleItem:
            case EType.ShovelItem:
            case EType.WateringCan:
            case EType.Fishingrod:
                
                break;
            default:
                break;
        }
        //UDebug.Print($"아이템 습득 성공 : {_slotIdx}번째 슬롯 | 획득 아이템 : {itemData.Name}");
        USound.PlaySfx(_sfxIdITemmoveSound);
    }
    /// <summary>
    /// 해당 슬롯 초기화.
    /// </summary>
    public void SlotClear()
    {
        if(_itemData == null) return;
        UDebug.Print($"SlotClear [{_slotIdx}]");
        _itemData = null;
        _stack = 0;
    }

    public void RemoveAmount(int amount)
    {
        if(amount < 0)
        {
            UDebug.Print("음수 값만큼 아이템을 제거할 수 없습니다.");
            return;
        }
        if (_stack == amount)
        {
            SlotClear();
            return;
        }
        _stack -= amount;

        if (_stack <= 0)
        {
            SlotClear();
            return;
        }
    }
    #endregion
}
