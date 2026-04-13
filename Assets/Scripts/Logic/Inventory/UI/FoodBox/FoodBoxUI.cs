/// <summary>
/// 클래스의 설계 의도입니다.
/// </summary>
public class FoodBoxUI : InventoryUI, IEscClosable
{
    public override void RefreshInventoryUI(int slotIndex, InventorySlot slotData)
    {
        if (IsValidSlotIndex(slotIndex) == false)
        {
            return;
        }

        _slotList[slotIndex].SetView(slotData);

        if (slotData.IsEmpty && _selectedIndex == slotIndex)
        {
            _selectedIndex = -1;
            RefreshSelectionView();
        }
    }
    public override void RefreshInventoryUI(Inventory inventory)
    {
        UDebug.Print($"current Request Inventory Size : {inventory.InventorySlots.Length}");

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
        base.Awake();
        _sfxId_InventoryOpen = Id.Sfx_Ui_ChestOpen_2;
        _sfxId_InventoryClose = Id.Sfx_Ui_ChestClosed_2;
        gameObject.SetActive(false);
    }
}
