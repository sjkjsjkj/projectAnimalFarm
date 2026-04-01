using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 인벤토리 슬롯 1칸의 표시와 클릭 전달을 담당합니다.
/// </summary>
public class UISlot : BaseMono
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("슬롯 UI")]
    [SerializeField] private Image _icon;
    [SerializeField] private TMP_Text _countText;
    [SerializeField] private GameObject _selectedFrame;
    [SerializeField] private Button _button;
    #endregion

    #region ─────────────────────────▶ 접근자 ◀─────────────────────────
    public int Index => _index;
    public RectTransform RectTr => _rectTr;
    public InventorySlotData SlotData => _slotData;
    #endregion

    #region ─────────────────────────▶ 중첩 타입 ◀─────────────────────────
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private UIInventory _owner;
    private RectTransform _rectTr;
    private InventorySlotData _slotData;
    private int _index = -1;
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    /// <summary>
    /// 버튼 클릭 시 부모 인벤토리에 현재 슬롯 인덱스를 전달합니다.
    /// </summary>
    private void OnClickSlot()
    {
        if (_owner == null)
        {
            return;
        }

        _owner.SelectSlot(_index);
    }

    /// <summary>
    /// 빈 슬롯 표현으로 갱신합니다.
    /// </summary>
    private void RefreshEmptyView()
    {
        if (_icon != null)
        {
            _icon.enabled = false;
            _icon.sprite = null;
        }

        if (_countText != null)
        {
            _countText.gameObject.SetActive(false);
            _countText.text = string.Empty;
        }
    }

    /// <summary>
    /// 아이템이 있는 슬롯 표현으로 갱신합니다.
    /// </summary>
    private void RefreshItemView(ItemSO item, int count)
    {
        if (_icon != null)
        {
            Sprite iconSprite = GetItemSprite(item);
            _icon.sprite = iconSprite;
            _icon.enabled = iconSprite != null;
        }

        if (_countText != null)
        {
            bool isShowCount = count > 1;
            _countText.gameObject.SetActive(isShowCount);
            _countText.text = isShowCount ? count.ToString() : string.Empty;
        }
    }

    /// <summary>
    /// ItemSO에서 아이콘 스프라이트를 안전하게 가져옵니다.
    /// 프로젝트의 공개 프로퍼티명이 아직 확정되지 않았을 때도 테스트 가능하도록 반사 접근을 허용합니다.
    /// </summary>
    private Sprite GetItemSprite(ItemSO item)
    {
        if (item == null)
        {
            return null;
        }

        System.Type type = item.GetType();

        PropertyInfo imageProperty = type.GetProperty("Image", BindingFlags.Public | BindingFlags.Instance);
        if (imageProperty != null && imageProperty.PropertyType == typeof(Sprite))
        {
            return imageProperty.GetValue(item) as Sprite;
        }

        PropertyInfo spriteProperty = type.GetProperty("Sprite", BindingFlags.Public | BindingFlags.Instance);
        if (spriteProperty != null && spriteProperty.PropertyType == typeof(Sprite))
        {
            return spriteProperty.GetValue(item) as Sprite;
        }

        FieldInfo imageField = type.GetField("_image", BindingFlags.NonPublic | BindingFlags.Instance);
        if (imageField != null && imageField.FieldType == typeof(Sprite))
        {
            return imageField.GetValue(item) as Sprite;
        }

        return null;
    }

    /// <summary>
    /// 버튼 이벤트를 안전하게 다시 연결합니다.
    /// </summary>
    private void BindButton()
    {
        if (_button == null)
        {
            return;
        }

        _button.onClick.RemoveListener(OnClickSlot);
        _button.onClick.AddListener(OnClickSlot);
    }
    #endregion

    #region ─────────────────────────▶ 외부 메서드 ◀─────────────────────────
    /// <summary>
    /// 슬롯의 초기 참조를 연결합니다.
    /// </summary>
    public void Initialize(UIInventory owner, int index)
    {
        _owner = owner;
        _index = index;
        _rectTr = transform as RectTransform;

        BindButton();
        SetSelected(false);
    }

    /// <summary>
    /// 슬롯 데이터를 받아 UI를 갱신합니다.
    /// </summary>
    public void SetData(InventorySlotData slotData)
    {
        _slotData = slotData;

        if (_slotData.IsEmpty)
        {
            RefreshEmptyView();
            return;
        }

        RefreshItemView(_slotData.Item, _slotData.Count);
    }

    /// <summary>
    /// 선택 여부를 갱신합니다.
    /// </summary>
    public void SetSelected(bool isSelected)
    {
        if (_selectedFrame != null)
        {
            _selectedFrame.SetActive(isSelected);
        }
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    private void Reset()
    {
        _button = GetComponent<Button>();
        _rectTr = transform as RectTransform;
    }
    #endregion
}
