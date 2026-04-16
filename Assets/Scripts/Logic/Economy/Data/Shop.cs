/// <summary>
/// 상점 클래스 입니다.
/// </summary>
public class Shop : IShopLogical
{
    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private ItemSO[] _sellItems;
    #endregion

    #region 생성자
    public Shop(ItemSO[] sellItems)
    {
        _sellItems = sellItems;
    }
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public ItemSO[] SellItems => _sellItems;

    public void ShowShopList()
    {
        UDebug.Print($"sellList : ");
        for (int i=0; i< _sellItems.Length; i++)
        {
            UDebug.Print($"{i} 번째 아이템 : {_sellItems[i].Name}");
        }
    }
    
    public bool TryBuyItem(string itemId, int buyPrice, int amount, out string message)
    {
        PlayerProvider player = DataManager.Ins.Player;
        
        if(player.Money < buyPrice)
        {
            OnFeedbackMessageRequested.Publish("플레이어의 소지금이 부족합니다.", EFeedbackMessageType.Failure,1.5f);
            message = "플레이어의 소지금이 부족합니다.";
            UDebug.Print(message);
            return false;
        }

        Inventory playerInven = InventoryManager.Ins.PlayerInventory;

        if (playerInven == null)
        {
            message = "플레이어 인벤토리를 찾을 수 없습니다.";
            UDebug.Print(message);
            return false;
        }

        if (!(playerInven.CheckSlots()))
        {
            OnFeedbackMessageRequested.Publish("인벤토리가 가득 찼습니다.", EFeedbackMessageType.Failure, 1.5f);
            message = "인벤토리가 가득 찼습니다.";
            UDebug.Print(message);
            return false;
        }

        player.TakeMoney(buyPrice);

        ItemSO tempItemSO = DatabaseManager.Ins.Item(itemId);
        ItemCollectionCoordinator.Ins.TryCollectItem(tempItemSO, amount);
        OnFeedbackMessageRequested.Publish($"{tempItemSO.Name} 구매 성공!", EFeedbackMessageType.Success, 1.5f);
        message = "구매 성공";
        UDebug.Print(message);
        return true;

    }
    public ItemSO GetItemSOShopSlotIdx(int slotIdx)
    {
        return _sellItems[slotIdx];
    }
    public bool TrySellItem(string itemId, int sellPrice, int amount, out string failMessage)
    {
        Inventory playerInven = InventoryManager.Ins.PlayerInventory;

        if(!(playerInven.TryRemoveItem(itemId)))
        {
            OnFeedbackMessageRequested.Publish("해당 아이템이 인벤토리에 없습니다.", EFeedbackMessageType.Failure, 1.5f);
            failMessage = "해당 아이템이 인벤토리에 없습니다.";
            return false;

        }
        failMessage = "판매 성공";
        //OnFeedbackMessageRequested.Publish($"판매 성공!", EFeedbackMessageType.Success, 1.5f);
        DataManager.Ins.Player.AddMoney(sellPrice);
        return true;
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────

    #endregion

}
