using UnityEngine;

/// <summary>
/// 플레이어 인벤토리의 UI 입니다.
/// </summary>
public class PlayerInventoryUI : InventoryUI
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [SerializeField] private RectTransform _contentsArea;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────

    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    public override void SetInfo(int invenSize)
    {
        UDebug.Print("InventoryUI SetInfo");

        _inventorySlotUIs = new InventoryUISlot[invenSize];

        for (int i = 0; i < invenSize; i++)
        {
            GameObject tempSlotUI = Instantiate(_inventorySlotUIPrefab);
            _inventorySlotUIs[i] = tempSlotUI.GetComponent<PlayerInventorySlotUI>();
            _inventorySlotUIs[i].SetInfo(this, i);
            tempSlotUI.transform.SetParent(_contentsArea);
        }
    }

    public override void RefreshInventoryUI(int slotIdx, InventorySlot invenSlot)
    {
        if(invenSlot == null || invenSlot.IsEmpty)
        {
            _inventorySlotUIs[slotIdx].SlotClear();
            return;
        }
        UDebug.Print($"Change Slot Idx : {slotIdx} | ItemData : {invenSlot.ItemSO.Name}");
        _inventorySlotUIs[slotIdx].SetInfo(invenSlot.ItemSO.Image, invenSlot.CurStack);
    }
    public override void RefreshInventoryUI(Inventory inventory)
    {
        InventorySlot[] tempInvenSlot = inventory.InventorySlots;
        for (int i = 0; i < tempInvenSlot.Length; i++)
        {
            if (tempInvenSlot[i].IsEmpty)
            {
                _inventorySlotUIs[i].SlotClear();
                continue;
            }
            _inventorySlotUIs[i].SetInfo(tempInvenSlot[i].ItemSO.Image, tempInvenSlot[i].CurStack);
        }
    }
    #endregion
}
