using System;
using UnityEngine;

/// <summary>
/// 메인 인벤토리 패널 전체를 관리합니다.
/// 슬롯 생성, 슬롯 UI 갱신, 선택, 드래그 앤 드롭 입력 전달, 액션 팝업 출력을 담당합니다.
/// 실제 아이템 데이터는 외부 시스템이 들고 있고,
/// 이 클래스는 외부에서 받은 표시 정보만 화면에 반영합니다.
/// </summary>
public class UIPlayerInventory : InventoryUI
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("팝업")]
    [SerializeField] protected RectTransform _popupRoot;
    [SerializeField] protected UIItemActionPopup _actionPopup;

    [Header("팝업 위치")]
    [SerializeField] protected float _popupOffsetX = 10f;
    [SerializeField] protected float _popupOffsetY = 0f;
    #endregion

    #region ─────────────────────────▶ 접근자 ◀─────────────────────────
    public int SelectedIndex => _selectedIndex;
    public int SlotCount => _slotList.Length;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private int _dragBeginIndex = -1;
    private bool _isDragging = false;
    private bool _isDropHandled = false;
    #endregion

    #region ─────────────────────────▶ 외부 메서드 ◀─────────────────────────
    /// <summary>
    /// 외부 시스템이 슬롯 사용을 요청받기 위한 이벤트입니다.
    /// 선택된 슬롯 인덱스를 전달합니다.
    /// </summary>
    public event Action<int> OnRequestUseSlot;

    /// <summary>
    /// 외부 시스템이 슬롯 버리기를 요청받기 위한 이벤트입니다.
    /// 선택된 슬롯 인덱스를 전달합니다.
    /// </summary>
    public event Action<int> OnRequestTrashSlot;

    /// <summary>
    /// 외부 시스템이 슬롯 스왑을 요청받기 위한 이벤트입니다.
    /// 시작 슬롯 인덱스와 도착 슬롯 인덱스를 전달합니다.
    /// </summary>
    public event Action<int, int> OnRequestSwapSlot;

    /// <summary>
    /// 특정 슬롯 하나를 갱신합니다.
    /// </summary>
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
            CloseActionPopup();
        }
    }

    /// <summary>
    /// 전체 슬롯을 한 번에 갱신합니다.
    /// </summary>
    public override void RefreshInventoryUI(Inventory inventory)
    {
        InventorySlot[] invenSlots = inventory.InventorySlots;
        for (int i = 0; i < _inventorySize; ++i)
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

        if (IsValidSlotIndex(_selectedIndex) == false)
        {
            _selectedIndex = -1;
        }
        RefreshSelectionView();
    }

    /// <summary>
    /// 패널을 닫습니다.
    /// </summary>
    public override void CloseUI()
    {
        _selectedIndex = -1;
        _dragBeginIndex = -1;
        _isDragging = false;
        _isDropHandled = false;
        RefreshSelectionView();
        CloseActionPopup();
        gameObject.SetActive(false);
    }


    /// <summary>
    /// 슬롯 클릭 시 선택 상태를 갱신합니다.
    /// 빈 슬롯은 아무 반응 없이 무시합니다.
    /// </summary>
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

        if (_actionPopup == null)
        {
            return;
        }

        Vector2 popupLocalPos = CalcPopupLocalPosition(targetSlot);
        _actionPopup.Show(popupLocalPos);
    }


    /// <summary>
    /// 액션 팝업을 닫습니다.
    /// </summary>
    public void CloseActionPopup()
    {
        if (_actionPopup == null)
        {
            return;
        }

        _actionPopup.Hide();
    }

    /// <summary>
    /// 팝업의 Use 버튼 입력입니다.
    /// </summary>
    public void OnClickUseFromPopup()
    {
        if (IsValidSlotIndex(_selectedIndex) == false)
        {
            return;
        }

        if (_slotList[_selectedIndex].IsEmpty)
        {
            return;
        }

        OnRequestUseSlot?.Invoke(_selectedIndex);
        ClearSelection();
    }

    /// <summary>
    /// 팝업의 Trash 버튼 입력입니다.
    /// </summary>
    public void OnClickTrashFromPopup()
    {
        if (IsValidSlotIndex(_selectedIndex) == false)
        {
            return;
        }

        if (_slotList[_selectedIndex].IsEmpty)
        {
            return;
        }

        OnRequestTrashSlot?.Invoke(_selectedIndex);
        ClearSelection();
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    
    /// <summary>
    /// 슬롯 프리팹을 기본 개수만큼 자동 생성합니다.
    /// </summary>
    

    /// <summary>
    /// 선택 슬롯 기준으로 팝업 위치를 계산합니다.
    /// </summary>
    private Vector2 CalcPopupLocalPosition(UISlot slot)
    {
        if (slot == null || _popupRoot == null)
        {
            return Vector2.zero;
        }

        RectTransform slotRectTr = slot.RectTr;
        RectTransform popupRectTr = _actionPopup != null
            ? _actionPopup.transform as RectTransform
            : null;

        Vector3[] slotCorners = new Vector3[4];
        slotRectTr.GetWorldCorners(slotCorners);

        // 우측 중앙 기준
        Vector3 slotRightCenterWorldPos = (slotCorners[2] + slotCorners[3]) * 0.5f;

        Canvas rootCanvas = _popupRoot.GetComponentInParent<Canvas>();
        Camera uiCamera = null;

        if (rootCanvas != null)
        {
            if (rootCanvas.renderMode == RenderMode.ScreenSpaceCamera ||
                rootCanvas.renderMode == RenderMode.WorldSpace)
            {
                uiCamera = rootCanvas.worldCamera;
            }
        }

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(uiCamera, slotRightCenterWorldPos);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _popupRoot,
            screenPoint,
            uiCamera,
            out Vector2 localPos);

        float halfPopupWidth = 0f;
        if (popupRectTr != null)
        {
            halfPopupWidth = popupRectTr.rect.width * 0.5f;
        }

        localPos.x += halfPopupWidth + _popupOffsetX;
        localPos.y += _popupOffsetY;

        return localPos;
    }

    
    public void ClearSelection()
    {
        _selectedIndex = -1;
        RefreshSelectionView();
        CloseActionPopup();
    }

    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    protected override void Awake()
    {
        base.Awake();

        if (_actionPopup != null)
        {
            _actionPopup.Initialize(this);
        }

        gameObject.SetActive(false);
    }
    #endregion
}
