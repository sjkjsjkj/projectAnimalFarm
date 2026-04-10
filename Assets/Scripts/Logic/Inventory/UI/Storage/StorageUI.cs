
/// <summary>
/// 창고의 UI 입니다.
/// </summary>
public class StorageUI : InventoryUI
{
    public override void RefreshInventoryUI(int slotIdx, InventorySlot invenSlot)
    {
        _inventorySlotUIs[slotIdx].SetInfo(invenSlot.ItemSO.Image, invenSlot.CurStack);
    }
    public override void RefreshInventoryUI(Inventory inventory)
    {
        InventorySlot[] invenSlots = inventory.InventorySlots;
        for (int i = 0; i < invenSlots.Length; ++i)
        {
            if (invenSlots[i].IsEmpty)
            {
                _slotList[i].ClearView();
            }
            else
            {
                _slotList[i].SetView(invenSlots[i]);
            }
        }
    }

    public override void SelectSlot(int slotIndex)
    {
        if (IsValidSlotIndex(slotIndex) == false)
        {
            return;
        }

        UISlot targetSlot = _slotList[slotIndex];
        if (targetSlot.IsEmpty)
        {
            return;
        }

        _selectedIndex = slotIndex;
        RefreshSelectionView();

        //if (_actionPopup == null)
        //{
        //    return;
        //}

        //Vector2 popupLocalPos = CalcPopupLocalPosition(targetSlot);
        //_actionPopup.Show(popupLocalPos);
    }
    private void Awake()
    {
        _sfxId_InventoryOpen = Id.Sfx_Ui_ChestOpen_2;
        _sfxId_InventoryClose = Id.Sfx_Ui_ChestClosed_2;
        gameObject.SetActive(false);
    }
}
