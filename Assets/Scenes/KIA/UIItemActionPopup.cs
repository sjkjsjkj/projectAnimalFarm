using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 슬롯 옆에 표시되는 액션 팝업입니다.
/// 실제 아이템 사용/버리기 로직은 갖지 않고,
/// 버튼 입력만 부모 인벤토리 패널에 전달합니다.
/// </summary>
public class UIItemActionPopup : BaseMono
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("팝업 버튼")]
    [SerializeField] private Button _useButton;
    [SerializeField] private Button _trashButton;
    [SerializeField] private Button _cancelButton;
    #endregion

    #region ─────────────────────────▶ 접근자 ◀─────────────────────────
    public bool IsOpen => gameObject.activeSelf;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private UIPlayerInventory _owner;
    private RectTransform _rectTr;
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    private void OnClickUse()
    {
        if (_owner == null)
        {
            return;
        }

        _owner.OnClickUseFromPopup();
    }

    private void OnClickTrash()
    {
        if (_owner == null)
        {
            return;
        }

        _owner.OnClickTrashFromPopup();
    }

    private void OnClickCancel()
    {
        if (_owner == null)
        {
            return;
        }

        _owner.ClearSelection();
    }

    private void BindButtons()
    {
        if (_useButton != null)
        {
            _useButton.onClick.RemoveListener(OnClickUse);
            _useButton.onClick.AddListener(OnClickUse);
        }

        if (_trashButton != null)
        {
            _trashButton.onClick.RemoveListener(OnClickTrash);
            _trashButton.onClick.AddListener(OnClickTrash);
        }

        if (_cancelButton != null)
        {
            _cancelButton.onClick.RemoveListener(OnClickCancel);
            _cancelButton.onClick.AddListener(OnClickCancel);
        }
    }
    #endregion

    #region ─────────────────────────▶ 외부 메서드 ◀─────────────────────────
    /// <summary>
    /// 팝업의 초기 참조를 연결합니다.
    /// </summary>
    public void Initialize(UIPlayerInventory owner)
    {
        _owner = owner;
        _rectTr = transform as RectTransform;

        BindButtons();
        Hide();
    }

    /// <summary>
    /// 팝업을 특정 로컬 좌표에 표시합니다.
    /// </summary>
    public void Show(Vector2 localPosition)
    {
        if (_rectTr != null)
        {
            _rectTr.anchoredPosition = localPosition;
        }

        gameObject.SetActive(true);
    }

    /// <summary>
    /// 팝업을 숨깁니다.
    /// </summary>
    public void Hide()
    {
        gameObject.SetActive(false);
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    private void Reset()
    {
        _rectTr = transform as RectTransform;
    }
    #endregion
}
