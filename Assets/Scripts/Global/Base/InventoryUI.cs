using UnityEngine;

/// <summary>
/// 인벤토리의 UI의 베이스 스크립트 입니다.
/// 플레이어의 인벤토리 / 창고 / 상점등이 이 베이스스크립트를 상속받아 사용할 예정입니다.
/// </summary>
public abstract class InventoryUI : BaseMono
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [SerializeField] protected GameObject _inventorySlotUIPrefab;
    [SerializeField] protected DragItemIconUI _dragIcon;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    protected int _curInvenIdx;

    protected int _inventorySize;
    protected InventoryUISlot[] _inventorySlotUIs;
    protected bool _isOpen = false;

    protected int _curDragIndex = -1;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public bool IsOpen => _isOpen;
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    public abstract void SetInfo(int invenSize);
 
    public void ShowUI()
    {
        gameObject.SetActive(true);
    }
    public void CloseUI()
    {
        gameObject.SetActive(false);
    }
    /// <summary>
    /// 하나의 슬롯만 최신화 하면 될 때 사용
    /// </summary>
    /// <param name="slotIdx">최신화 할 UI 슬롯의 인덱스</param>
    /// <param name="invenSlot">최신화 할 UI 슬롯에 들어갈 데이터</param>
    public abstract void RefreshInventoryUI(int slotIdx, InventorySlot invenSlot);
    public abstract void RefreshInventoryUI(Inventory inventory);
    public void SetToggleUI()
    {
        _isOpen = !_isOpen;
        if(_isOpen)
        {
            ShowUI();
        }
        else
        {
            CloseUI();
        }
    }

    #region 드래그 관련
    //슬롯 드래그가 시작 되었을 때
    public void BeginDrag(int index)
    {
        ItemSO tempItemSO = InventoryManager.Ins.Inventories[_curInvenIdx].InventorySlots[index].ItemSO;

        if (tempItemSO == null)
        {
            return;
        }

        _curDragIndex = index;

        _dragIcon.Show(tempItemSO.Image);
        _dragIcon.SetPosition(Input.mousePosition);
    }
    //슬롯 드래그 중일 때
    public void DragIng()
    {
        if (_curDragIndex == -1)
        {
            return;
        }
        _dragIcon.SetPosition(Input.mousePosition);
    }
    //슬롯 드래그 끝났을 때
    public void EndDrag()
    {
        _curDragIndex = -1;
        _dragIcon.Hide();
    }
    public void DragDrop(int targetIndex)
    {
        if (_curDragIndex == -1)
        {
            return;
        }

        InventoryManager.Ins.Inventories[_curInvenIdx].SwapItemSlot(_curDragIndex, targetIndex);

        EndDrag();
    }
    #endregion

    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────

    #endregion

    #region ─────────────────────────▶ 중첩 타입 ◀─────────────────────────

    #endregion
}
