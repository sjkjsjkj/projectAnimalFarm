using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

/// <summary>
/// 메인 인벤토리 패널 전체를 관리합니다.
/// 슬롯 생성, 데이터 갱신, 선택, 액션 팝업 출력을 담당합니다.
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

    [Header("테스트 아이템 SO")]
    [SerializeField] private ItemSO _testItemA;
    [SerializeField] private ItemSO _testItemB;
    [SerializeField] private ItemSO _testItemC;
    #endregion

    #region ─────────────────────────▶ 접근자 ◀─────────────────────────
    public int SelectedIndex => _selectedIndex;
    #endregion

    #region ─────────────────────────▶ 중첩 타입 ◀─────────────────────────
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private readonly List<InventorySlotData> _slotDataList = new();
    private int _selectedIndex = -1;
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    /// <summary>
    /// 기존에 남아 있는 슬롯 오브젝트를 제거합니다.
    /// 자동 생성 구조로 전환할 때 중복 생성을 막기 위함입니다.
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
            _slotList.Add(slot);
        }
    }

    /// <summary>
    /// 테스트용 슬롯 데이터를 생성합니다.
    /// </summary>
    private void CreateTestSlotData()
    {
        _slotDataList.Clear();

        int slotCount = _slotList.Count;
        for (int i = 0; i < slotCount; ++i)
        {
            _slotDataList.Add(InventorySlotData.CreateEmpty());
        }

        if (slotCount > 0 && _testItemA != null)
        {
            _slotDataList[0] = new InventorySlotData(_testItemA, 12);
        }

        if (slotCount > 1 && _testItemB != null)
        {
            _slotDataList[1] = new InventorySlotData(_testItemB, 8);
        }

        if (slotCount > 2 && _testItemC != null)
        {
            _slotDataList[2] = new InventorySlotData(_testItemC, 1);
        }

        if (slotCount > 3 && _testItemA != null)
        {
            _slotDataList[3] = new InventorySlotData(_testItemA, 24);
        }

        if (slotCount > 4 && _testItemB != null)
        {
            _slotDataList[4] = new InventorySlotData(_testItemB, 3);
        }

        if (slotCount > 15 && _testItemC != null)
        {
            _slotDataList[15] = new InventorySlotData(_testItemC, 2);
        }

        if (slotCount > 16 && _testItemA != null)
        {
            _slotDataList[16] = new InventorySlotData(_testItemA, 30);
        }
    }

    /// <summary>
    /// 전체 슬롯 UI를 현재 데이터 기준으로 갱신합니다.
    /// </summary>
    private void RefreshSlots()
    {
        int slotCount = _slotList.Count;

        for (int i = 0; i < slotCount; ++i)
        {
            InventorySlotData slotData = i < _slotDataList.Count
                ? _slotDataList[i]
                : InventorySlotData.CreateEmpty();

            _slotList[i].SetData(slotData);
            _slotList[i].SetSelected(i == _selectedIndex);
        }
    }

    /// <summary>
    /// 선택 슬롯 기준으로 팝업 위치를 계산합니다.
    /// PopupRoot 기준 로컬 좌표로 변환합니다.
    /// </summary>
    private Vector2 CalcPopupLocalPosition(UISlot slot)
    {
        if (slot == null || _popupRoot == null)
        {
            return Vector2.zero;
        }

        RectTransform slotRectTr = slot.RectTr;
        Vector3 worldPos = slotRectTr.position;
        Camera uiCamera = UCamera.UICamera;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _popupRoot,
            RectTransformUtility.WorldToScreenPoint(uiCamera, worldPos),
            uiCamera,
            out Vector2 localPos);

        localPos.x += 70f;
        localPos.y -= 10f;

        return localPos;
    }

    /// <summary>
    /// ItemSO의 표시 이름을 안전하게 가져옵니다.
    /// </summary>
    private string GetItemDisplayName(ItemSO item)
    {
        if (item == null)
        {
            return "None";
        }

        System.Type type = item.GetType();

        PropertyInfo nameProperty = type.GetProperty("Name", BindingFlags.Public | BindingFlags.Instance);
        if (nameProperty != null && nameProperty.PropertyType == typeof(string))
        {
            object value = nameProperty.GetValue(item);
            if (value is string stringValue && string.IsNullOrEmpty(stringValue) == false)
            {
                return stringValue;
            }
        }

        FieldInfo nameField = type.GetField("_name", BindingFlags.NonPublic | BindingFlags.Instance);
        if (nameField != null && nameField.FieldType == typeof(string))
        {
            object value = nameField.GetValue(item);
            if (value is string stringValue && string.IsNullOrEmpty(stringValue) == false)
            {
                return stringValue;
            }
        }

        return item.name;
    }
    #endregion

    #region ─────────────────────────▶ 외부 메서드 ◀─────────────────────────
    /// <summary>
    /// 슬롯 데이터를 기준으로 전체 인벤토리 UI를 갱신합니다.
    /// </summary>
    public void Refresh()
    {
        RefreshSlots();
    }

    /// <summary>
    /// 슬롯 선택을 갱신하고 액션 팝업을 표시합니다.
    /// </summary>
    public void SelectSlot(int index)
    {
        if (index < 0 || index >= _slotList.Count)
        {
            return;
        }

        _selectedIndex = index;
        RefreshSlots();

        InventorySlotData slotData = _slotDataList[index];
        if (slotData.IsEmpty)
        {
            CloseActionPopup();
            return;
        }

        if (_actionPopup == null)
        {
            return;
        }

        Vector2 popupLocalPos = CalcPopupLocalPosition(_slotList[index]);
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
    /// 현재는 테스트용 로그만 출력합니다.
    /// </summary>
    public void OnClickUseFromPopup()
    {
        if (_selectedIndex < 0 || _selectedIndex >= _slotDataList.Count)
        {
            return;
        }

        InventorySlotData slotData = _slotDataList[_selectedIndex];
        if (slotData.IsEmpty)
        {
            return;
        }

        string itemName = GetItemDisplayName(slotData.Item);
        UDebug.Print($"사용 버튼 클릭: {itemName}, 수량: {slotData.Count}");
    }

    /// <summary>
    /// 팝업의 Trash 버튼 입력입니다.
    /// 현재는 테스트용 로그만 출력합니다.
    /// </summary>
    public void OnClickTrashFromPopup()
    {
        if (_selectedIndex < 0 || _selectedIndex >= _slotDataList.Count)
        {
            return;
        }

        InventorySlotData slotData = _slotDataList[_selectedIndex];
        if (slotData.IsEmpty)
        {
            return;
        }

        string itemName = GetItemDisplayName(slotData.Item);
        UDebug.Print($"버리기 버튼 클릭: {itemName}, 수량: {slotData.Count}");
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

        CreateTestSlotData();
        Refresh();
    }

    private void OnDisable()
    {
        CloseActionPopup();
        _selectedIndex = -1;
        Refresh();
    }
    #endregion
}
