using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

/// <summary>
/// 메인 인벤토리 패널 전체를 관리합니다.
/// 슬롯 생성, 슬롯 UI 갱신, 선택, 드래그 앤 드롭 입력 전달, 액션 팝업 출력을 담당합니다.
/// 실제 아이템 데이터는 외부 시스템이 들고 있고,
/// 이 클래스는 외부에서 받은 표시 정보만 화면에 반영합니다.
/// </summary>
public class UIInventory : BaseMono
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("슬롯")]
    [SerializeField] private Transform _slotContainer;
    [SerializeField] private UISlot _slotPrefab;
    [SerializeField] private int _defaultSlotCount = 15;
    [SerializeField] private List<UISlot> _slotList = new();

    [Header("팝업")]
    [SerializeField] private RectTransform _popupRoot;
    [SerializeField] private UIItemActionPopup _actionPopup;

    [Header("팝업 위치")]
    [SerializeField] private float _popupOffsetX = 10f;
    [SerializeField] private float _popupOffsetY = 0f;
    #endregion

    #region ─────────────────────────▶ 접근자 ◀─────────────────────────
    public int SelectedIndex => _selectedIndex;
    public int SlotCount => _slotList.Count;
    public bool IsOpen => gameObject.activeSelf;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private int _selectedIndex = -1;
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
    public void RefreshInventoryUI(int slotIndex, InventorySlotData slotData)
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
    public void RefreshInventoryUI(IReadOnlyList<InventorySlotData> slotDatas)
    {
        int slotCount = _slotList.Count;

        for (int i = 0; i < slotCount; ++i)
        {
            InventorySlotData slotData = InventorySlotData.CreateEmpty();

            if (slotDatas != null && i < slotDatas.Count)
            {
                slotData = slotDatas[i];
            }

            _slotList[i].SetView(slotData);
        }

        if (IsValidSlotIndex(_selectedIndex) == false)
        {
            _selectedIndex = -1;
        }

        RefreshSelectionView();
    }

    /// <summary>
    /// 패널을 엽니다.
    /// </summary>
    public void OpenUI()
    {
        gameObject.SetActive(true);
    }

    /// <summary>
    /// 패널을 닫습니다.
    /// </summary>
    public void CloseUI()
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
    /// 패널 활성 상태를 토글합니다.
    /// </summary>
    public void ToggleUI()
    {
        if (IsOpen)
        {
            CloseUI();
            return;
        }

        OpenUI();
    }

    /// <summary>
    /// 슬롯 클릭 시 선택 상태를 갱신합니다.
    /// 빈 슬롯은 아무 반응 없이 무시합니다.
    /// </summary>
    public void SelectSlot(int slotIndex)
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
    /// 슬롯 드래그 시작을 패널에 알립니다.
    /// 시작 슬롯이 빈 슬롯이면 드래그를 무시합니다.
    /// </summary>
    public void BeginDragSlot(int slotIndex)
    {
        if (IsValidSlotIndex(slotIndex) == false)
        {
            _dragBeginIndex = -1;
            _isDragging = false;
            _isDropHandled = false;
            return;
        }

        if (_slotList[slotIndex].IsEmpty)
        {
            _dragBeginIndex = -1;
            _isDragging = false;
            _isDropHandled = false;
            return;
        }

        _dragBeginIndex = slotIndex;
        _selectedIndex = slotIndex;
        _isDragging = true;
        _isDropHandled = false;

        RefreshSelectionView();
        CloseActionPopup();
    }

    /// <summary>
    /// 드래그 종료 알림입니다.
    /// Drop에서 실제 처리가 끝나므로 여기서는 유지합니다.
    /// </summary>
    public void EndDragSlot()
    {
        StartCoroutine(CoEndDragCleanup());
    }

    /// <summary>
    /// 슬롯 드롭을 패널에 알립니다.
    /// 같은 슬롯이면 무시하고, 다른 슬롯이면 외부 시스템에 스왑 요청을 전달합니다.
    /// </summary>
    public void DropSlot(int targetSlotIndex)
    {
        if (IsValidSlotIndex(_dragBeginIndex) == false)
        {
            return;
        }

        if (IsValidSlotIndex(targetSlotIndex) == false)
        {
            _isDragging = false;
            _dragBeginIndex = -1;
            ClearSelection();
            return;
        }

        if (_dragBeginIndex == targetSlotIndex)
        {
            _isDragging = false;
            _dragBeginIndex = -1;
            ClearSelection();
            return;
        }

        _isDropHandled = true;
        OnRequestSwapSlot?.Invoke(_dragBeginIndex, targetSlotIndex);

        _isDragging = false;
        _dragBeginIndex = -1;
        ClearSelection();
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
    /// 기존 슬롯 오브젝트를 모두 제거합니다.
    /// </summary>
    private void ClearSlots()
    {
        _slotList.Clear();

        if (_slotContainer == null)
        {
            return;
        }

        int childCount = _slotContainer.childCount;
        for (int i = childCount - 1; i >= 0; --i)
        {
            Transform child = _slotContainer.GetChild(i);
            if (child == null)
            {
                continue;
            }

            Destroy(child.gameObject);
        }
    }

    /// <summary>
    /// 슬롯 프리팹을 기본 개수만큼 자동 생성합니다.
    /// </summary>
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

        ClearSlots();

        for (int i = 0; i < _defaultSlotCount; ++i)
        {
            UISlot slot = Instantiate(_slotPrefab, _slotContainer);
            slot.gameObject.name = $"UISlot_{i:D2}";
            slot.Initialize(this, i);
            slot.ClearView();
            slot.SetSelected(false);
            _slotList.Add(slot);
        }
    }

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

    /// <summary>
    /// 모든 슬롯의 선택 표시를 갱신합니다.
    /// </summary>
    private void RefreshSelectionView()
    {
        for (int i = 0; i < _slotList.Count; ++i)
        {
            _slotList[i].SetSelected(i == _selectedIndex);
        }
    }

    /// <summary>
    /// 인덱스가 슬롯 범위 안에 있는지 확인합니다.
    /// </summary>
    private bool IsValidSlotIndex(int index)
    {
        return index >= 0 && index < _slotList.Count;
    }

    public void ClearSelection()
    {
        _selectedIndex = -1;
        RefreshSelectionView();
        CloseActionPopup();
    }

    /// <summary>
    /// 드래그 종료 후 한 프레임 뒤에 드롭 처리 여부를 보고 선택 상태를 정리합니다.
    /// </summary>
    private IEnumerator CoEndDragCleanup()
    {
        yield return null;

        if (_isDragging == false)
        {
            yield break;
        }

        if (_isDropHandled)
        {
            yield break;
        }

        _isDragging = false;
        _dragBeginIndex = -1;
        ClearSelection();
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    protected override void Awake()
    {
        base.Awake();

        CreateSlots();

        if (_actionPopup != null)
        {
            _actionPopup.Initialize(this);
        }

        gameObject.SetActive(false);
    }
    #endregion
}
