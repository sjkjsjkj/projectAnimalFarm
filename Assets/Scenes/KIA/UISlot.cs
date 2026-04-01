using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 인벤토리 슬롯 1칸의 표시와 입력 전달을 담당합니다.
/// 실제 아이템 데이터는 들고 있지 않고,
/// 외부에서 전달받은 아이콘/수량만 화면에 표시합니다.
/// </summary>
public class UISlot : BaseMono, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("슬롯 UI")]
    [SerializeField] private Image _icon;
    [SerializeField] private TMP_Text _countText;

    [Header("선택된 프레임")]
    [SerializeField] private GameObject _selectedFrame;
    [SerializeField] private Button _button;
    #endregion

    #region ─────────────────────────▶ 접근자 ◀─────────────────────────
    public int Index => _index;
    public RectTransform RectTr => _rectTr;
    public bool IsEmpty => _isEmpty;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private UIInventory _owner;
    private RectTransform _rectTr;
    private int _index = -1;
    private bool _isEmpty = true;
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
        ClearView();
        SetSelected(false);
    }

    /// <summary>
    /// 빈 슬롯 표시로 갱신합니다.
    /// </summary>
    public void ClearView()
    {
        _isEmpty = true;

        if (_icon != null)
        {
            _icon.sprite = null;
            _icon.enabled = false;
        }

        if (_countText != null)
        {
            _countText.text = string.Empty;
            _countText.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 슬롯 표시를 갱신합니다.
    /// </summary>
    public void SetView(InventorySlotData slotData)
    {
        if (slotData.IsEmpty)
        {
            ClearView();
            return;
        }

        _isEmpty = false;

        if (_icon != null)
        {
            _icon.sprite = slotData.Icon;
            _icon.enabled = slotData.Icon != null;
        }

        if (_countText != null)
        {
            bool isShowCount = slotData.Count > 1;
            _countText.gameObject.SetActive(isShowCount);
            _countText.text = isShowCount ? slotData.Count.ToString() : string.Empty;
        }
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
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_owner == null)
        {
            return;
        }

        _owner.BeginDragSlot(_index);
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 드래그 이벤트 유지용
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_owner == null)
        {
            return;
        }

        _owner.EndDragSlot();
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (_owner == null)
        {
            return;
        }

        _owner.DropSlot(_index);
    }

    private void Reset()
    {
        _button = GetComponent<Button>();
        _rectTr = transform as RectTransform;
    }
    #endregion
}
