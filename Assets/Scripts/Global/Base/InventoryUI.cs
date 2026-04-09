using UnityEngine;

/// <summary>
/// 인벤토리의 UI의 베이스 스크립트 입니다.
/// 플레이어의 인벤토리 / 창고 / 상점등이 이 베이스스크립트를 상속받아 사용할 예정입니다.
/// </summary>
public abstract class InventoryUI : BaseMono
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("슬롯")]
    [SerializeField] protected Transform _slotContainer;
    [SerializeField] protected UISlot _slotPrefab;

    [SerializeField] protected DragItemIconUI _dragIcon;

    [SerializeField] protected UISlot[] _slotList;// = new();

    [SerializeField] private int TestCurrentId;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    protected int _curInvenIdx;

    protected int _inventorySize;
    protected InventoryUISlot[] _inventorySlotUIs;
    protected bool _isOpen = false;

    protected int _curDragIndex;
    protected int _selectedIndex;

    protected string _sfxId_InventoryOpen;
    protected string _sfxId_InventoryClose;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public bool IsOpen => _isOpen;
    public int InventoryIdx => _curInvenIdx;
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    public void SetSize(int invenSize)
    {
        _inventorySize = invenSize;
        CreateSlots();
    }
    #region UI ON/OFF
    public void ShowUI()
    {
        gameObject.SetActive(true);
    }
    public virtual void CloseUI()
    {
        gameObject.SetActive(false);
        _curDragIndex = -1;
    }
    
    #endregion
    public void SetCurrentOpenInventoryId(int id)
    {
        _curInvenIdx = id;
        TestCurrentId = _curInvenIdx;
    }
    private void CreateSlots()
    {
        if (_slotContainer == null)
        {
            UDebug.Print("SlotContainer가 연결되지 않았습니다.", LogType.Warning);
            return;
        }

        if (_slotPrefab == null)
        {
            UDebug.Print("Slot 프리팹이 연결되지 않았습니다.", LogType.Warning);
            return;
        }
        UDebug.Print($"current Inventory : {InventoryIdx} | {_inventorySize}");
        _slotList = new UISlot[_inventorySize];

        //ClearSlots();

        for (int i = 0; i < _inventorySize; ++i)
        {
            UISlot slot = Instantiate(_slotPrefab, _slotContainer);
            slot.gameObject.name = $"UISlot_{i:D2}";
            slot.Initialize(this, i);
            slot.ClearView();
            slot.SetSelected(false);
            //_slotList.Add(slot);
            _slotList[i] = slot;
        }
    }


    #region UI 시각 최신화
    /// <summary>
    /// 하나의 슬롯만 최신화 하면 될 때 사용
    /// </summary>
    /// <param name="slotIdx">최신화 할 UI 슬롯의 인덱스</param>
    /// <param name="invenSlot">최신화 할 UI 슬롯에 들어갈 데이터</param>
    public abstract void RefreshInventoryUI(int slotIdx, InventorySlot invenSlot);
    public abstract void RefreshInventoryUI(Inventory inventory);

    #endregion
    public abstract void SelectSlot(int slotIdx);

    /// <summary>
    /// 인덱스가 슬롯 범위 안에 있는지 확인합니다.
    /// </summary>
    protected bool IsValidSlotIndex(int index)
    {
        return index >= 0 && index < _slotList.Length;
    }

    /// <summary>
    /// 모든 슬롯의 선택 표시를 갱신합니다.
    /// </summary>
    protected void RefreshSelectionView()
    {
        for (int i = 0; i < _slotList.Length; ++i)
        {
            //_slotList[i].SetSelected(i == _selectedIndex);
            _slotList[i].SetSelected(false);
        }
    }

    public virtual void SetToggleUI()
    {
        _isOpen = !_isOpen;
        if (_isOpen)
        {
            USound.PlaySfx(_sfxId_InventoryOpen);
            ShowUI();
        }
        else
        {
            USound.PlaySfx(_sfxId_InventoryClose);
            CloseUI();
        }
        //UDebug.Print($"current Stats : {_isOpen}");
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
        _dragIcon.transform.SetParent(InventoryManager.Ins.GlobalCanvas);
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
        //UDebug.Print($"Mouse Drop] cur inven id = {_curInvenIdx} | {targetIndex} ");
        //InventoryManager.Ins.Inventories[_curInvenIdx].SwapItemSlot(_curDragIndex, targetIndex);

        EndDrag();
    }

    #endregion

    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    private void OnDestroy()
    {
        UDebug.Print($"파괴되었습니다.. 난 Dontdestroy인데 {UnityEngine.StackTraceUtility.ExtractStackTrace()}", LogType.Warning);
    }

    protected override void Awake()
    {
        base.Awake();

        //CreateSlots();

        gameObject.SetActive(false);
    }
    #endregion

    #region ─────────────────────────▶ 중첩 타입 ◀─────────────────────────

    #endregion
}
