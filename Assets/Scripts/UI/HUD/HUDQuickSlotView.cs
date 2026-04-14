using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HUD 퀵슬롯 5칸의 실제 표시를 담당하는 뷰
/// 1. 각 칸의 아이콘을 갱신한다.
/// 2. 현재 선택된 퀵슬롯을 강조 표시한다.
/// 3. 버튼이 연결되어 있으면 클릭 입력도 외부로 전달한다.
/// </summary>
public class HUDQuickSlotView : BaseMono
{
    [Serializable]
    private class QuickSlotUI
    {
        public RectTransform root;
        public Image background;
        public Image icon;
        public GameObject selectedObject;
        public Button button;
    }

    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("퀵슬롯 UI 참조")]
    [SerializeField] private QuickSlotUI[] _slots = new QuickSlotUI[ToolQuickSlotUtility.QUICK_SLOT_COUNT];

    [Header("선택 색상")]
    [SerializeField] private Color _normalBackgroundColor = Color.white;
    [SerializeField] private Color _selectedBackgroundColor = new Color(1f, 0.9f, 0.35f, 1f);

    [Header("빈 슬롯 처리")]
    [SerializeField] private bool _hideIconWhenEmpty = true;
    #endregion

    #region ─────────────────────────▶ 이벤트 ◀─────────────────────────
    public event Action<int> OnQuickSlotClicked;
    #endregion

    #region ─────────────────────────▶ 외부 메서드 ◀─────────────────────────
    public void SetSlots(ToolQuickSlotUtility.QuickSlotToolData[] slotDatas)
    {
        int count = _slots != null ? _slots.Length : 0;

        for (int i = 0; i < count; i++)
        {
            ToolQuickSlotUtility.QuickSlotToolData data =
                ToolQuickSlotUtility.QuickSlotToolData.CreateEmpty(i, ToolQuickSlotUtility.GetToolTypeByQuickSlotIndex(i));

            if (slotDatas != null && i < slotDatas.Length)
            {
                data = slotDatas[i];
            }

            SetSlot(i, data);
        }
    }

    public void SetSlot(int quickSlotIndex, ToolQuickSlotUtility.QuickSlotToolData data)
    {
        if (IsValidSlotIndex(quickSlotIndex) == false)
        {
            return;
        }

        QuickSlotUI slotUI = _slots[quickSlotIndex];
        if (slotUI == null)
        {
            return;
        }

        if (slotUI.icon != null)
        {
            slotUI.icon.sprite = data.HasItem ? data.Icon : null;

            if (_hideIconWhenEmpty)
            {
                slotUI.icon.enabled = data.HasItem && data.Icon != null;
            }
            else
            {
                slotUI.icon.enabled = true;
                slotUI.icon.color = data.HasItem ? Color.white : new Color(1f, 1f, 1f, 0f);
            }
        }
    }

    public void SetSelected(int quickSlotIndex)
    {
        if (_slots == null)
        {
            return;
        }

        for (int i = 0; i < _slots.Length; i++)
        {
            bool isSelected = (i == quickSlotIndex);
            ApplySelectedState(i, isSelected);
        }
    }

    public void ClearAll()
    {
        if (_slots == null)
        {
            return;
        }

        for (int i = 0; i < _slots.Length; i++)
        {
            SetSlot(i, ToolQuickSlotUtility.QuickSlotToolData.CreateEmpty(i, ToolQuickSlotUtility.GetToolTypeByQuickSlotIndex(i)));
            ApplySelectedState(i, false);
        }
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    private bool IsValidSlotIndex(int index)
    {
        return _slots != null && index >= 0 && index < _slots.Length;
    }

    private void ApplySelectedState(int quickSlotIndex, bool isSelected)
    {
        if (IsValidSlotIndex(quickSlotIndex) == false)
        {
            return;
        }

        QuickSlotUI slotUI = _slots[quickSlotIndex];
        if (slotUI == null)
        {
            return;
        }

        if (slotUI.background != null)
        {
            slotUI.background.color = isSelected ? _selectedBackgroundColor : _normalBackgroundColor;
        }

        if (slotUI.selectedObject != null)
        {
            slotUI.selectedObject.SetActive(isSelected);
        }
    }

    private void BindButtons()
    {
        if (_slots == null)
        {
            return;
        }

        for (int i = 0; i < _slots.Length; i++)
        {
            QuickSlotUI slotUI = _slots[i];
            if (slotUI == null || slotUI.button == null)
            {
                continue;
            }

            int capturedIndex = i;
            slotUI.button.onClick.RemoveAllListeners();
            slotUI.button.onClick.AddListener(() =>
            {
                OnQuickSlotClicked?.Invoke(capturedIndex);
            });
        }
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    protected override void Awake()
    {
        base.Awake();
        BindButtons();
    }

    private void OnValidate()
    {
        if (_slots == null || _slots.Length != ToolQuickSlotUtility.QUICK_SLOT_COUNT)
        {
            _slots = new QuickSlotUI[ToolQuickSlotUtility.QUICK_SLOT_COUNT];
        }
    }

    private void Reset()
    {
        if (_slots == null || _slots.Length != ToolQuickSlotUtility.QUICK_SLOT_COUNT)
        {
            _slots = new QuickSlotUI[ToolQuickSlotUtility.QUICK_SLOT_COUNT];
        }

        for (int i = 0; i < ToolQuickSlotUtility.QUICK_SLOT_COUNT; i++)
        {
            if (_slots[i] == null)
            {
                _slots[i] = new QuickSlotUI();
            }

            Transform slotRoot = transform.Find($"QuickSlot_{i + 1}");
            if (slotRoot == null)
            {
                continue;
            }

            _slots[i].root = slotRoot as RectTransform;

            Transform backgroundTr = slotRoot.Find("Background");
            if (backgroundTr != null)
            {
                _slots[i].background = backgroundTr.GetComponent<Image>();
            }

            Transform iconTr = slotRoot.Find("Icon");
            if (iconTr != null)
            {
                _slots[i].icon = iconTr.GetComponent<Image>();
            }

            _slots[i].button = slotRoot.GetComponent<Button>();
        }
    }
    #endregion
}
