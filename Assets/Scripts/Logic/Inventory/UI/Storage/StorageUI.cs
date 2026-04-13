
/// <summary>
/// 창고의 UI 입니다.
/// </summary>
public class StorageUI : InventoryUI, IEscClosable
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

    public void CloseUi()
    {
        SetToggleUI(); // 부모의 토글로 닫기
    }

    // ★ SetActive(true)  → 자동으로 EscManager 등록
    private void OnEnable()
    {
        EscManager.Ins.Enter(this);
    }

    // ★ SetActive(false) → 자동으로 EscManager 해제
    private void OnDisable()
    {
        EscManager.Ins.Exit(this);
    }

    private new void Awake()
    {
        base.Awake(); // ← 부모 Awake 실행 유지
        _sfxId_InventoryOpen = Id.Sfx_Ui_ChestOpen_2;
        _sfxId_InventoryClose = Id.Sfx_Ui_ChestClosed_2;
        gameObject.SetActive(false);
    }
}
