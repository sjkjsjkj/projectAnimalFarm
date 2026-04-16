using System;


/// <summary>
/// 인벤토리 입니다. 플레이어의 인벤토리 및 창고, 상점들을 이것으로 관리합니다.
/// </summary>
public class Inventory
{
    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    protected int _inventoryIdx;
    protected int _inventorySize;
    protected InventorySlot[] _inventorySlots;
    protected EInventoryType _inventoryType;    
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    //public int InventorySize => _inventorySize;
    public InventorySlot[] InventorySlots => _inventorySlots;

    public event Action<EInventoryType, InventorySlot> OnChangeSlot;
    public event Action<EInventoryType, Inventory> OnChangeSlots;
    public EInventoryType InvenType => _inventoryType;
    #endregion

    #region ─────────────────────────▶   생성자   ◀─────────────────────────
    public Inventory(int inventorySize, EInventoryType inventoryType, int inventoryIdx)
    {
        //UDebug.Print("인벤토리 생성");
        _inventorySize = inventorySize;
        _inventorySlots = new InventorySlot[inventorySize];
        _inventoryType = inventoryType;
        _inventoryIdx = inventoryIdx;

        for(int i=0; i<_inventorySlots.Length;  i++)
        {
            //UDebug.Print($"인벤토리 생성자 체크 : {i}");
            _inventorySlots[i] = new InventorySlot(i);
        }
        
    }
    #endregion

    #region ─────────────────────────▶ 내부 ◀─────────────────────────

    protected virtual void OnChangeSlotEvent(EInventoryType invenType, InventorySlot slot )
    {
        OnChangeSlot?.Invoke(invenType, slot);
    }
    #endregion

    #region ─────────────────────────▶ 외부 공개 ◀─────────────────────────

    public void NotifyUseItem(int invenSlotIdx)
    {
        TryUseItem(invenSlotIdx);
    }
    public void NotifyTrashItem(int invenSlotIdx)
    {
        ItemSlotClear(invenSlotIdx);
    }

    public bool TryUseItem(int invenSlotIdx)
    {
        if (_inventorySlots[invenSlotIdx].IsEmpty)
        {
            UDebug.Print("설마");
            return false;
        }
        if (_inventorySlots[invenSlotIdx].ItemSO.ItemUseContext.Length==0)
        {
            UDebug.Print("여기라고?");
            return false;
        }

        _inventorySlots[invenSlotIdx].ItemSO.Use();
        _inventorySlots[invenSlotIdx].RemoveAmount(1);

        OnChangeSlot?.Invoke(EInventoryType.PlayerInventory, _inventorySlots[invenSlotIdx]);

        return false;
    }
    public virtual bool TryGetItem(ItemSO itemData , int amount = 1)
    {
        if (UDebug.IsNull(itemData))
        {
            UDebug.Print("아이템 데이터가 비어있습니다.");
            return false;
        }
        int itemInputSlot = CheckSlots(itemData, amount);

        if (itemInputSlot != -1)
        {
            //UDebug.Print("인벤UI 최신화!");
            OnChangeSlot?.Invoke(_inventoryType, _inventorySlots[itemInputSlot]);

            if (_inventoryType == EInventoryType.PlayerInventory)
            {
                //UDebug.Print($"OnPlayerGetItem 발행 | id = {itemData.Id}, amount = {amount}");
                OnPlayerGetItem.Publish(itemData.Id, amount);
            }

            return true;
        }
        return false;
    }
    public bool CheckSlots()
    {
        for(int i=0; i < _inventorySlots.Length; i++)
        {
            if (_inventorySlots[i].IsEmpty)
            {
                return true;
            }
        }
        return false;
    }
    public int CheckSlots(ItemSO itemData, int itemStack = 1)
    {
        //겹칠 수 없는 아이템이라면
        if (itemData.MaxStack == 1)
        {
            for (int i = 0; i < _inventorySlots.Length; i++)
            {
                //빈자리를 찾는 즉시 아이템 넣고 성공 반환
                if (_inventorySlots[i].IsEmpty)
                {
                    _inventorySlots[i].SetItem(itemData, itemStack);
                    return i;//성공시 해당 슬롯의 번호를 반환.
                }
            }
            //인벤토리에 빈자리 없음.

            UDebug.Print("인벤토리가 가득 찼습니다.");

            //TODO : 풀인벤토리 이슈와 관련된 UI 호출 요청.
            return -1; //실패 반환
        }
        //겹칠 수 있는 아이템이라면
        else
        {
            int emptySlotIndex = -1; //아이템 겹치는 아이템이 존재하지 않는다면 사용될 슬롯의 인덱스
            //먼저 아이템 슬롯 전체에서 같은 아이템이 있는지 확인.
            for (int i = 0; i < _inventorySlots.Length; i++)
            {
                //아이템 겹치는 아이템이 존재하지 않는다면 사용될 슬롯의 인덱스 구해놓기
                if ( _inventorySlots[i].IsEmpty )
                {
                    if (emptySlotIndex != -1)
                    {
                        continue;
                    }
                    emptySlotIndex = i;
                    
                    continue;
                }
                //인벤토리에서 겹치는 아이템 찾음.
                if (_inventorySlots[i].SlotItemId.CompareTo(itemData.Id) == 0)
                {
                    //해당 슬롯이 꽉 찼다면 다음.
                    if (_inventorySlots[i].CurStack >= itemData.MaxStack)
                    {
                        continue;
                    }
                    //같은 아이템이고, 슬롯이 꽉차지도 않았다면 아이템 획득 후, 성공 반환.
                    _inventorySlots[i].SetItem(itemData);
                    //OnPlayerGetItem.Publish(itemData.Id, itemStack);
                    return i;//성공시 해당 슬롯의 번호를 반환.
                }
            }
            //겹치는 아이템을 못찾았고, 빈자리는 찾았다면
            if (emptySlotIndex != -1)
            {
                //그냥 빈 자리에 넣음.
                _inventorySlots[emptySlotIndex].SetItem(itemData);
                //OnPlayerGetItem.Publish(itemData.Id, itemStack);
                return emptySlotIndex;
            }
            //인벤토리에 겹치는 아이템도 없고, 빈자리도 없다면.
            //TODO : 풀인벤토리 이슈와 관련된 UI 호출 요청.
            return -1; //실패 반환.
        }
    }

    public void SwapItemSlot(int slotIdx1, int slotIdx2)
    {
        UDebug.Print($"Start : {slotIdx1} | End : {slotIdx2} ");
        if(slotIdx1 == slotIdx2)
        {
            UDebug.Print("시작과 끝이 같은 슬롯");
            return;
        }
        if (InventorySlots[slotIdx2].IsEmpty)
        {
            UDebug.Print("빈 슬롯으로 드래그");
            InventorySlots[slotIdx2].SetItem(InventorySlots[slotIdx1].ItemSO, InventorySlots[slotIdx1].CurStack);
            InventorySlots[slotIdx1].SlotClear();

            OnChangeSlot?.Invoke(_inventoryType, _inventorySlots[slotIdx1]);
            OnChangeSlot?.Invoke(_inventoryType, _inventorySlots[slotIdx2]);

            return;
        }

        UDebug.Print("스왑");
        ItemSO tempSo = _inventorySlots[slotIdx1].ItemSO;
        int tempStackCount = _inventorySlots[slotIdx1].CurStack;

        _inventorySlots[slotIdx1].SlotClear();
        _inventorySlots[slotIdx1].SetItem(_inventorySlots[slotIdx2].ItemSO, _inventorySlots[slotIdx2].CurStack);
        _inventorySlots[slotIdx2].SlotClear();
        _inventorySlots[slotIdx2].SetItem(tempSo, tempStackCount);


        OnChangeSlot?.Invoke(_inventoryType, _inventorySlots[slotIdx1]);
        OnChangeSlot?.Invoke(_inventoryType, _inventorySlots[slotIdx2]);

    }

    //해당 아이템이 있는 인벤토리의 슬롯 번호를 반환
    public int FindItemToSlot(string itemId)
    {
        for(int i= 0; i< _inventorySlots.Length; i++)
        {
            if (_inventorySlots[i].IsEmpty)
            {
                continue;
            }
            if (_inventorySlots[i].ItemSO.Id.CompareTo(itemId)==0)
            {
                return _inventorySlots[i].CurStack;
            }
        }
        return -1;
    }
    //Todo:
    //인벤토리에 해당 아이템이 총 몇개 있는지 반환하는 메서드 작성
    public int FindItemToInventory(string itemId)
    {
        int totalCount = 0;

        for(int i=0; i<_inventorySlots.Length; i++)
        {
            if (_inventorySlots[i].IsEmpty)
            {
                continue;
            }
            if (_inventorySlots[i].ItemSO.Id.CompareTo(itemId) == 0)
            {
                totalCount += _inventorySlots[i].CurStack;
            }
        }

        return totalCount;
    }
    //해당 타입의 아이템이 인벤토리에 있는지 (ex 먹이 / 씨앗 ) 확인
    public int FindItemType(EType findType, int startSlotIdx = 0)
    {
        for(int i= startSlotIdx; i< _inventorySlots.Length; i++)
        {
            if (_inventorySlots[i].IsEmpty)
            {
                continue;
            }
            if (_inventorySlots[i].ItemSO.Type == findType)
            {
                return i;
            }
        }
        return -1;
    }
    //특정 아이템을 하나 받아옴 (인덱스 기준 가장 앞에 있는 녀석)
    public ItemSO GetItemType(EType findType)
    {
        if(findType == EType.None)
        {
            UDebug.Print("돌아가세요 손님");
            return null;
        }

        int slotIdx = FindItemType(findType);

        if (slotIdx == -1)
        {
            UDebug.Print("해당 타입의 아이템이 없습니다.");
            return null;
        }

        ItemSO tempItemSO = _inventorySlots[slotIdx].ItemSO;

        if(tempItemSO == null)
        {
            UDebug.Print("그럴리가 없는데");
            return null;
        }
        _inventorySlots[slotIdx].RemoveAmount(1);

        OnChangeSlots?.Invoke(_inventoryType, this);

        return tempItemSO;
    }
    public void SetItemSlot(InventorySlot invenSlot, int slotIdx)
    {
        if(invenSlot.IsEmpty)
        {
            return;
        }
        ItemSlotClear(slotIdx);
        _inventorySlots[slotIdx] = invenSlot;
        if (_inventorySlots[slotIdx].IsEmpty)
        {
            return;
        }
        UDebug.Print($"{slotIdx} 번째 슬롯의 현재 데이터 : {_inventorySlots[slotIdx].ItemSO.Name}");
        //OnChangeSlot?.Invoke(EInventoryType.PlayerInventory, _inventorySlots[slotIdx]);
        OnChangeSlots?.Invoke(_inventoryType, this);
    }

    //아이템제거 시도
    public bool TryRemoveItem(string itemId, int amount =1)
    {
        int slotIdx = FindItemToSlot(itemId);
        
        if(slotIdx == -1)
        {
            UDebug.Print("해당 아이템을 찾을 수 없습니다.");
            return false;
        }
        
        if(CheckHasItem(itemId,amount))
        {
            RemoveItem(itemId, amount);
            return true;
        }
        else
        {
            UDebug.Print("아이템의 개수가 부족합니다.");
            return false;
        }
    }
    //실제로 지우는 메서드
    private void RemoveItem(string itemId, int amount =1)
    {
        for (int i = 0; i < _inventorySlots.Length; i++)
        {
            if (_inventorySlots[i].IsEmpty)
            {
                continue;
            }
            //Todo : 조건 변경
            //ㄴ같으면 들어오는것이 아니라, 다르면 컨티뉴로
            if (_inventorySlots[i].ItemSO.Id.CompareTo(itemId)==0)
            {
                int removeValue = (int)MathF.Min(amount, _inventorySlots[i].CurStack);

                _inventorySlots[i].RemoveAmount(removeValue);
                amount -= removeValue;

                if (amount <= 0) break;
            }
        }
        
        OnChangeSlots?.Invoke(_inventoryType, this);
    }

    //슬롯 전체 제거
    public void ItemSlotClear(int slotIdx)
    {
        _inventorySlots[slotIdx].SlotClear();

        // OnChangeSlot?.Invoke(EInventoryType.PlayerInventory, _inventorySlots[slotIdx]);
        OnChangeSlots?.Invoke(_inventoryType, this);
    }
    //해당 아이템이 인벤토리에 충분하게 있는지 체크
    public bool CheckHasItem(string itemId, int count)
    {
        int hasItemCount = 0;
        
        hasItemCount = FindItemToInventory(itemId);
        UDebug.Print($"인벤토리에 현재 아이템 개수 : {hasItemCount} | 필요 개수 : {count}");
        if (hasItemCount >= count)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    #endregion
}
