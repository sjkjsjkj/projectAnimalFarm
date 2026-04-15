using System;

/// <summary>
/// 클래스의 설계 의도입니다.
/// </summary>
[Serializable]
public class FoodBox : Inventory
{
    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
   
    #endregion

    #region ─────────────────────────▶   생성자   ◀─────────────────────────
    public FoodBox(int inventorySize, EInventoryType inventoryType, int inventoryIdx) : base(inventorySize, inventoryType, inventoryIdx)
    {
        _inventorySize = inventorySize;
        _inventoryType = inventoryType;
        _inventoryIdx = inventoryIdx;
        _inventorySlots = new InventorySlot[inventorySize];
    }
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public bool CheckItemType(ItemSO itemSO)
    {
        return itemSO.Type == EType.FeedItem;
        //return itemSO.Type == EType.ProductItem;
    }

    public override bool TryGetItem(ItemSO itemData, int amount = 1)
    {
        if( !CheckItemType(itemData))
        {
            return false;
        }
        return base.TryGetItem(itemData, amount);
    }

    public bool TryFindFeed()
    {
        for (int i = 0; i < _inventorySize; i++)
        {
            if (_inventorySlots[i].IsEmpty)
            {
                continue;
            }
            if (!(_inventorySlots[i].ItemSO as FeedItemSO))
            {
                UDebug.Print("일어날 수 없는 일", UnityEngine.LogType.Assert);
                continue;
            }
            return true;
        }
        return false;
    }
    public FeedItemSO ReturnFeed()
    {
        for(int i=0; i<_inventorySize; i++)
        {
            if (_inventorySlots[i].IsEmpty)
            {
                continue;
            }
            if (!(_inventorySlots[i].ItemSO is FeedItemSO feedItemSO))
            {
                UDebug.Print("일어날 수 없는 일", UnityEngine.LogType.Assert);
                continue;
            }
            else
            {
                _inventorySlots[i].RemoveAmount(1);
                OnChangeSlotEvent(EInventoryType.FoodBox, _inventorySlots[i]);
                //OnChangeSlot?.Invoke(EInventoryType.FoodBox, _inventorySlots[i]);
                return feedItemSO;
            }
        }
        UDebug.Print("이 메서드는 TryFindFeed와 함께 사용하세요.",UnityEngine.LogType.Assert);
        return null;
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    
    #endregion

}
