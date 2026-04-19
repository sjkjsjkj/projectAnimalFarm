using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 테스트 전용 인벤토리 프리젠터입니다.
/// 
/// [중요]
/// - 이 스크립트는 UI 테스트용입니다.
/// - 실제 인벤토리 시스템이 들어오면 삭제해도 됩니다.
/// - 하이어라키에 아이템 오브젝트를 두지 않고,
///   미리 등록한 테스트 아이템 데이터를 키 입력으로 획득하는 방식입니다.
/// 
/// 테스트 기능
/// 1. 숫자 1 / 2 / 3 키로 테스트 아이템 획득
/// 2. 선택 슬롯 수량 증가 / 감소
/// 3. 드래그 드롭 이동 / 스왑 / 합치기
/// 4. 아이템 변경 후 자동 정렬
/// </summary>
public class UIInventoryDebugPresenter : BaseMono
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("대상 UI")]
    [SerializeField] private UIPlayerInventory _uiInventory;

    [Header("테스트 아이템 목록")]
    [SerializeField] private InventoryDebugItemData _debugItem1;
    [SerializeField] private InventoryDebugItemData _debugItem2;
    [SerializeField] private InventoryDebugItemData _debugItem3;

    [Header("테스트 시작 상태")]
    [SerializeField] private bool _isStartWithOneItem = true;
    [SerializeField] private int _startItemNumber = 1;
    [SerializeField] private int _startItemCount = 1;

    [Header("테스트 입력 키")]
    [SerializeField] private Key _pickupItem1Key = Key.Digit1;
    [SerializeField] private Key _pickupItem2Key = Key.Digit2;
    [SerializeField] private Key _pickupItem3Key = Key.Digit3;
    [SerializeField] private Key _increaseSelectedCountKey = Key.Equals;
    [SerializeField] private Key _decreaseSelectedCountKey = Key.Minus;
    #endregion

    #region ─────────────────────────▶ 중첩 타입 ◀─────────────────────────
    /// <summary>
    /// 테스트 전용 런타임 슬롯 데이터입니다.
    /// 실제 프로젝트에서는 실제 인벤토리 데이터 구조로 대체하면 됩니다.
    /// </summary>
    private struct RuntimeSlotData
    {
        #region ─────────────────────────▶ 접근자 ◀─────────────────────────
        public string Name => _name;
        public Sprite Icon => _icon;
        public int Count => _count;
        public int MaxStack => _maxStack;
        public bool IsEmpty => _icon == null || _count <= 0;
        #endregion

        #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
        private string _name;
        private Sprite _icon;
        private int _count;
        private int _maxStack;
        #endregion

        #region ─────────────────────────▶ 외부 메서드 ◀─────────────────────────
        public RuntimeSlotData(string name, Sprite icon, int count, int maxStack)
        {
            _name = name;
            _icon = icon;
            _count = count;
            _maxStack = Mathf.Max(1, maxStack);
        }

        public void SetCount(int count)
        {
            _count = count;
        }

        public void Clear()
        {
            _name = string.Empty;
            _icon = null;
            _count = 0;
            _maxStack = 1;
        }

        public static RuntimeSlotData CreateEmpty()
        {
            return new RuntimeSlotData(string.Empty, null, 0, 1);
        }
        #endregion
    }
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private readonly List<RuntimeSlotData> _runtimeSlotList = new();
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    /// <summary>
    /// 번호에 맞는 테스트 아이템 데이터를 반환합니다.
    /// </summary>
    private bool TryGetDebugItemData(int itemNumber, out InventoryDebugItemData itemData)
    {
        itemData = default;

        switch (itemNumber)
        {
            case 1:
                itemData = _debugItem1;
                return itemData.IsValid;

            case 2:
                itemData = _debugItem2;
                return itemData.IsValid;

            case 3:
                itemData = _debugItem3;
                return itemData.IsValid;

            default:
                return false;
        }
    }

    /// <summary>
    /// 테스트용 런타임 슬롯 배열을 초기화합니다.
    /// </summary>
    private void InitializeRuntimeSlots()
    {
        _runtimeSlotList.Clear();

        if (_uiInventory == null)
        {
            UDebug.Print("[UIInventoryDebugPresenter] UIInventory가 연결되지 않았습니다.", LogType.Warning);
            return;
        }

        if (_uiInventory.SlotCount <= 0)
        {
            UDebug.Print("[UIInventoryDebugPresenter] UI 슬롯이 아직 생성되지 않았습니다.", LogType.Warning);
            return;
        }

        for (int i = 0; i < _uiInventory.SlotCount; ++i)
        {
            _runtimeSlotList.Add(RuntimeSlotData.CreateEmpty());
        }

        // TEST ONLY
        // 시작할 때 선택한 번호의 테스트 아이템 1종을 미리 넣어두는 코드입니다.
        if (_isStartWithOneItem)
        {
            if (TryGetDebugItemData(_startItemNumber, out InventoryDebugItemData startItemData))
            {
                if (_runtimeSlotList.Count > 0)
                {
                    int startCount = Mathf.Max(1, _startItemCount);
                    _runtimeSlotList[0] = new RuntimeSlotData(
                        startItemData.Name,
                        startItemData.Icon,
                        startCount,
                        startItemData.MaxStack);
                }
            }
        }
    }

    /// <summary>
    /// 현재 런타임 데이터를 UI 표시용 데이터로 변환합니다.
    /// </summary>
    private List<InventorySlotData> BuildViewData()
    {
        List<InventorySlotData> viewDataList = new(_runtimeSlotList.Count);

        for (int i = 0; i < _runtimeSlotList.Count; ++i)
        {
            RuntimeSlotData slotData = _runtimeSlotList[i];

            if (slotData.IsEmpty)
            {
                viewDataList.Add(InventorySlotData.CreateEmpty());
                continue;
            }

            viewDataList.Add(new InventorySlotData(slotData.Icon, slotData.Count));
        }

        return viewDataList;
    }

    /// <summary>
    /// UI 전체를 새로고침합니다.
    /// </summary>
    private void RefreshUI()
    {
        if (_uiInventory == null)
        {
            return;
        }

        //_uiInventory.RefreshInventoryUI(BuildViewData());
    }

    /// <summary>
    /// 유효한 슬롯 인덱스인지 확인합니다.
    /// </summary>
    private bool IsValidIndex(int index)
    {
        return index >= 0 && index < _runtimeSlotList.Count;
    }

    /// <summary>
    /// 두 슬롯이 같은 아이템인지 확인합니다.
    /// </summary>
    private bool IsSameItem(RuntimeSlotData a, RuntimeSlotData b)
    {
        if (a.IsEmpty || b.IsEmpty)
        {
            return false;
        }

        return a.Name == b.Name && a.Icon == b.Icon;
    }

    /// <summary>
    /// 비어 있지 않은 슬롯을 앞쪽으로 당겨 자동 정렬합니다.
    /// 왼쪽 위(0번)부터 빈칸 없이 채워지는 형태입니다.
    /// </summary>
    private void NormalizeSlots()
    {
        List<RuntimeSlotData> sortedList = new(_runtimeSlotList.Count);

        for (int i = 0; i < _runtimeSlotList.Count; ++i)
        {
            if (_runtimeSlotList[i].IsEmpty)
            {
                continue;
            }

            sortedList.Add(_runtimeSlotList[i]);
        }

        while (sortedList.Count < _runtimeSlotList.Count)
        {
            sortedList.Add(RuntimeSlotData.CreateEmpty());
        }

        _runtimeSlotList.Clear();
        _runtimeSlotList.AddRange(sortedList);
    }

    /// <summary>
    /// 지정한 번호의 테스트 아이템을 획득합니다.
    /// 같은 아이템이 있으면 먼저 중첩하고, 없으면 빈 슬롯에 넣습니다.
    /// 처리 후 자동 정렬합니다.
    /// </summary>
    private void PickupDebugItem(int itemNumber, int addCount)
    {
        if (TryGetDebugItemData(itemNumber, out InventoryDebugItemData itemData) == false)
        {
            UDebug.Print($"[UIInventoryDebugPresenter] {itemNumber}번 테스트 아이템 데이터가 비어 있습니다.", LogType.Warning);
            return;
        }

        int remainCount = Mathf.Max(1, addCount);

        for (int i = 0; i < _runtimeSlotList.Count; ++i)
        {
            RuntimeSlotData slotData = _runtimeSlotList[i];
            if (slotData.IsEmpty)
            {
                continue;
            }

            bool isSameItem = slotData.Name == itemData.Name && slotData.Icon == itemData.Icon;
            if (isSameItem == false)
            {
                continue;
            }

            int remainSpace = slotData.MaxStack - slotData.Count;
            if (remainSpace <= 0)
            {
                continue;
            }

            int moveCount = Mathf.Min(remainSpace, remainCount);
            slotData.SetCount(slotData.Count + moveCount);
            _runtimeSlotList[i] = slotData;

            remainCount -= moveCount;
            if (remainCount <= 0)
            {
                UDebug.Print($"[UIInventoryDebugPresenter] {itemNumber}번 아이템 획득 : {itemData.Name}");
                NormalizeSlots();
                RefreshUI();
                return;
            }
        }

        for (int i = 0; i < _runtimeSlotList.Count; ++i)
        {
            if (_runtimeSlotList[i].IsEmpty == false)
            {
                continue;
            }

            int insertCount = Mathf.Min(itemData.MaxStack, remainCount);
            _runtimeSlotList[i] = new RuntimeSlotData(
                itemData.Name,
                itemData.Icon,
                insertCount,
                itemData.MaxStack);

            remainCount -= insertCount;
            if (remainCount <= 0)
            {
                UDebug.Print($"[UIInventoryDebugPresenter] {itemNumber}번 아이템 획득 : {itemData.Name}");
                NormalizeSlots();
                RefreshUI();
                return;
            }
        }

        UDebug.Print("[UIInventoryDebugPresenter] 인벤토리가 가득 찼습니다.", LogType.Warning);
        NormalizeSlots();
        RefreshUI();
    }

    /// <summary>
    /// 선택된 슬롯 수량을 증가시킵니다.
    /// TEST ONLY
    /// </summary>
    private void IncreaseSelectedSlotCount(int addCount)
    {
        if (_uiInventory == null)
        {
            return;
        }

        int selectedIndex = _uiInventory.SelectedIndex;
        if (IsValidIndex(selectedIndex) == false)
        {
            return;
        }

        RuntimeSlotData slotData = _runtimeSlotList[selectedIndex];
        if (slotData.IsEmpty)
        {
            return;
        }

        int nextCount = Mathf.Min(slotData.MaxStack, slotData.Count + Mathf.Max(1, addCount));
        slotData.SetCount(nextCount);
        _runtimeSlotList[selectedIndex] = slotData;

        RefreshUI();
    }

    /// <summary>
    /// 선택된 슬롯 수량을 감소시킵니다.
    /// 수량이 0이 되면 제거 후 자동 정렬합니다.
    /// TEST ONLY
    /// </summary>
    private void DecreaseSelectedSlotCount(int subCount)
    {
        if (_uiInventory == null)
        {
            return;
        }

        int selectedIndex = _uiInventory.SelectedIndex;
        if (IsValidIndex(selectedIndex) == false)
        {
            return;
        }

        RuntimeSlotData slotData = _runtimeSlotList[selectedIndex];
        if (slotData.IsEmpty)
        {
            return;
        }

        int nextCount = slotData.Count - Mathf.Max(1, subCount);
        if (nextCount <= 0)
        {
            slotData.Clear();
        }
        else
        {
            slotData.SetCount(nextCount);
        }

        _runtimeSlotList[selectedIndex] = slotData;
        NormalizeSlots();
        RefreshUI();
    }

    /// <summary>
    /// 드래그 드롭 시 같은 아이템이면 최대 중첩 수까지 합칩니다.
    /// </summary>
    private void MergeSlotData(int fromIndex, int toIndex)
    {
        RuntimeSlotData fromSlot = _runtimeSlotList[fromIndex];
        RuntimeSlotData toSlot = _runtimeSlotList[toIndex];

        int remainSpace = toSlot.MaxStack - toSlot.Count;
        if (remainSpace <= 0)
        {
            return;
        }

        int moveCount = Mathf.Min(remainSpace, fromSlot.Count);

        toSlot.SetCount(toSlot.Count + moveCount);
        fromSlot.SetCount(fromSlot.Count - moveCount);

        if (fromSlot.Count <= 0)
        {
            fromSlot.Clear();
        }

        _runtimeSlotList[fromIndex] = fromSlot;
        _runtimeSlotList[toIndex] = toSlot;
    }

    /// <summary>
    /// 슬롯 두 개를 서로 바꿉니다.
    /// </summary>
    private void SwapSlotData(int fromIndex, int toIndex)
    {
        RuntimeSlotData temp = _runtimeSlotList[fromIndex];
        _runtimeSlotList[fromIndex] = _runtimeSlotList[toIndex];
        _runtimeSlotList[toIndex] = temp;
    }

    /// <summary>
    /// UI의 Use 요청을 처리합니다.
    /// </summary>
    private void HandleUseSlot(int slotIndex)
    {
        if (IsValidIndex(slotIndex) == false)
        {
            return;
        }

        RuntimeSlotData slotData = _runtimeSlotList[slotIndex];
        if (slotData.IsEmpty)
        {
            return;
        }

        int nextCount = slotData.Count - 1;
        if (nextCount <= 0)
        {
            slotData.Clear();
            UDebug.Print($"[UIInventoryDebugPresenter] 슬롯 {slotIndex} 아이템 전부 사용");
        }
        else
        {
            slotData.SetCount(nextCount);
            UDebug.Print($"[UIInventoryDebugPresenter] 슬롯 {slotIndex} 아이템 사용, 남은 수량 : {nextCount}");
        }

        _runtimeSlotList[slotIndex] = slotData;
        NormalizeSlots();
        RefreshUI();
    }

    /// <summary>
    /// UI의 Trash 요청을 처리합니다.
    /// </summary>
    private void HandleTrashSlot(int slotIndex)
    {
        if (IsValidIndex(slotIndex) == false)
        {
            return;
        }

        RuntimeSlotData slotData = _runtimeSlotList[slotIndex];
        if (slotData.IsEmpty)
        {
            return;
        }

        slotData.Clear();
        _runtimeSlotList[slotIndex] = slotData;

        UDebug.Print($"[UIInventoryDebugPresenter] 슬롯 {slotIndex} 아이템 버리기");

        NormalizeSlots();
        RefreshUI();
    }

    /// <summary>
    /// UI의 Swap 요청을 처리합니다.
    /// 빈 슬롯이면 이동,
    /// 같은 아이템이면 합치기,
    /// 다른 아이템이면 스왑합니다.
    /// </summary>
    private void HandleSwapSlot(int fromIndex, int toIndex)
    {
        if (IsValidIndex(fromIndex) == false || IsValidIndex(toIndex) == false)
        {
            return;
        }

        if (fromIndex == toIndex)
        {
            return;
        }

        RuntimeSlotData fromSlot = _runtimeSlotList[fromIndex];
        RuntimeSlotData toSlot = _runtimeSlotList[toIndex];

        if (fromSlot.IsEmpty)
        {
            return;
        }

        if (toSlot.IsEmpty)
        {
            _runtimeSlotList[toIndex] = fromSlot;
            _runtimeSlotList[fromIndex] = RuntimeSlotData.CreateEmpty();

            UDebug.Print($"[UIInventoryDebugPresenter] 슬롯 이동 : {fromIndex} -> {toIndex}");
            RefreshUI();
            return;
        }

        if (IsSameItem(fromSlot, toSlot))
        {
            MergeSlotData(fromIndex, toIndex);
            UDebug.Print($"[UIInventoryDebugPresenter] 같은 아이템 합치기 : {fromIndex} -> {toIndex}");
            RefreshUI();
            return;
        }

        SwapSlotData(fromIndex, toIndex);
        UDebug.Print($"[UIInventoryDebugPresenter] 슬롯 스왑 : {fromIndex} <-> {toIndex}");
        RefreshUI();
    }

    /// <summary>
    /// 테스트 입력을 처리합니다.
    /// TEST ONLY
    /// </summary>
    private void HandleDebugKeyboardInput()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        if (keyboard[_pickupItem1Key].wasPressedThisFrame)
        {
            PickupDebugItem(1, 1);
        }

        if (keyboard[_pickupItem2Key].wasPressedThisFrame)
        {
            PickupDebugItem(2, 1);
        }

        if (keyboard[_pickupItem3Key].wasPressedThisFrame)
        {
            PickupDebugItem(3, 1);
        }

        if (keyboard[_increaseSelectedCountKey].wasPressedThisFrame)
        {
            IncreaseSelectedSlotCount(1);
        }

        if (keyboard[_decreaseSelectedCountKey].wasPressedThisFrame)
        {
            DecreaseSelectedSlotCount(1);
        }
    }

    /// <summary>
    /// UI 이벤트를 구독합니다.
    /// </summary>
    private void BindUIEvents()
    {
        if (_uiInventory == null)
        {
            UDebug.Print("[UIInventoryDebugPresenter] UIInventory가 연결되지 않았습니다.", LogType.Warning);
            return;
        }

        _uiInventory.OnRequestUseSlot -= HandleUseSlot;
        _uiInventory.OnRequestUseSlot += HandleUseSlot;

        _uiInventory.OnRequestTrashSlot -= HandleTrashSlot;
        _uiInventory.OnRequestTrashSlot += HandleTrashSlot;

        _uiInventory.OnRequestSwapSlot -= HandleSwapSlot;
        _uiInventory.OnRequestSwapSlot += HandleSwapSlot;
    }

    /// <summary>
    /// UI 이벤트 구독을 해제합니다.
    /// </summary>
    private void UnbindUIEvents()
    {
        if (_uiInventory == null)
        {
            return;
        }

        _uiInventory.OnRequestUseSlot -= HandleUseSlot;
        _uiInventory.OnRequestTrashSlot -= HandleTrashSlot;
        _uiInventory.OnRequestSwapSlot -= HandleSwapSlot;
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        BindUIEvents();
        InitializeRuntimeSlots();
        RefreshUI();
    }

    private void Update()
    {
        // TEST ONLY
        // 숫자키와 수량 조절 키를 이용한 디버그 입력 처리
        HandleDebugKeyboardInput();
    }

    private void OnDestroy()
    {
        UnbindUIEvents();
    }
    #endregion
}
