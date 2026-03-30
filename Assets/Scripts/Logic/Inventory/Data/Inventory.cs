/// <summary>
/// 인벤토리 입니다. 플레이어의 인벤토리 및 창고, 상점들을 이것으로 관리합니다.
/// </summary>
public class Inventory
{
    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private int _inventorySize;
    private InventorySlot[] _inventorySlots;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    //public int InventorySize => _inventorySize;
    //public InventorySlot[] InventorySlots => _inventorySlots;
    #endregion

    #region ─────────────────────────▶   생성자   ◀─────────────────────────
    public Inventory(int inventorySize)
    {
        _inventorySize = inventorySize;
        _inventorySlots = new InventorySlot[inventorySize];
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    public bool TryGetItem(ItemSO itemData)
    {
        UDebug.IsNull(itemData);

        //겹칠 수 없는 아이템이라면
        if(itemData.MaxStack == 1)
        {
            for(int i=0; i< _inventorySlots.Length; i++)
            {
                //빈자리를 찾는 즉시 아이템 넣고 성공 반환
                if (_inventorySlots[i].IsEmpty)
                {
                    _inventorySlots[i].GetItem(itemData);

                    return true;
                }
            }
            //인벤토리에 빈자리 없음.
            UDebug.Print("인벤토리가 가득 찼습니다.");
            //TODO : 풀인벤토리 이슈와 관련된 UI 호출 요청.
            return false; //실패 반환
        }
        //겹칠 수 있는 아이템이라면
        else
        {
            int emptySlotIndex = -1; //아이템 겹치는 아이템이 존재하지 않는다면 사용될 슬롯의 인덱스
            //먼저 아이템 슬롯 전체에서 같은 아이템이 있는지 확인.
            for (int i = 0; i < _inventorySlots.Length; i++)
            {
                //아이템 겹치는 아이템이 존재하지 않는다면 사용될 슬롯의 인덱스 구해놓기
                if (_inventorySlots[i].IsEmpty)
                {
                    if(emptySlotIndex != -1)
                    {
                        continue;
                    }
                    emptySlotIndex = i;
                }
                //인벤토리에서 겹치는 아이템 찾음.
                if (_inventorySlots[i].SlotItemId.CompareTo(itemData.Id)==0)
                {
                    //해당 슬롯이 꽉 찼다면 다음.
                    if (_inventorySlots[i].CurStack >= itemData.MaxStack)
                    {
                        continue;
                    }
                    //같은 아이템이고, 슬롯이 꽉차지도 않았다면 아이템 획득 후, 성공 반환.
                    _inventorySlots[i].GetItem(itemData);
                    return true;
                }
            }
            //겹치는 아이템을 못찾았고, 빈자리는 찾았다면
            if(emptySlotIndex != -1)
            {
                //그냥 빈 자리에 넣음.
                _inventorySlots[emptySlotIndex].GetItem(itemData);
            }
            //인벤토리에 겹치는 아이템도 없고, 빈자리도 없다면.
            //TODO : 풀인벤토리 이슈와 관련된 UI 호출 요청.
            return false; //실패 반환.
        }
    }
    #endregion
}
