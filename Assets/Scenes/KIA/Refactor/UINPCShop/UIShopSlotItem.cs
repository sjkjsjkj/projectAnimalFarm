using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 상점 슬롯 프리팹 1개의 버튼 입력과 아이콘 표시를 담당합니다.
/// </summary>
public class UIShopSlotItem : BaseMono
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("상점 슬롯 참조")]
    [SerializeField] private Image _itemIcon;
    [SerializeField] private Button _buyButton;
    [SerializeField] private Button _sellButton;
    [SerializeField] private TextMeshProUGUI _buyText;
    [SerializeField] private TextMeshProUGUI _sellText;
    #endregion

    #region ─────────────────────────▶ 접근자 ◀─────────────────────────
    /// <summary>
    /// 현재 슬롯 인덱스입니다.
    /// </summary>
    public int SlotIndex => _slotIndex;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    /// <summary>
    /// 현재 슬롯 번호입니다.
    /// </summary>
    private int _slotIndex;

    /// <summary>
    /// 구매 버튼 클릭 시 호출할 외부 콜백입니다.
    /// </summary>
    private Action<int> _onClickBuy;

    /// <summary>
    /// 판매 버튼 클릭 시 호출할 외부 콜백입니다.
    /// </summary>
    private Action<int> _onClickSell;

    /// <summary>
    /// Setup 호출 여부입니다.
    /// </summary>
    private bool _isSetup;
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    /// <summary>
    /// 버튼 이벤트를 연결합니다.
    /// RemoveListener 후 AddListener로 중복 등록을 방지합니다.
    /// </summary>
    private void BindButtons()
    {
        if (_buyButton != null)
        {
            _buyButton.onClick.RemoveListener(HandleClickBuy);
            _buyButton.onClick.AddListener(HandleClickBuy);
        }

        if (_sellButton != null)
        {
            _sellButton.onClick.RemoveListener(HandleClickSell);
            _sellButton.onClick.AddListener(HandleClickSell);
        }
    }

    /// <summary>
    /// 구매 버튼 클릭을 처리합니다.
    /// </summary>
    private void HandleClickBuy()
    {
        if (_isSetup == false)
        {
            return;
        }

        _onClickBuy?.Invoke(_slotIndex);
    }

    /// <summary>
    /// 판매 버튼 클릭을 처리합니다.
    /// </summary>
    private void HandleClickSell()
    {
        if (_isSetup == false)
        {
            return;
        }

        _onClickSell?.Invoke(_slotIndex);
    }
    #endregion

    #region ─────────────────────────▶ 외부 메서드 ◀─────────────────────────
    /// <summary>
    /// 슬롯 표시 정보와 버튼 콜백을 세팅합니다.
    /// </summary>
    /// <param name="slotIndex">슬롯 번호</param>
    /// <param name="iconSprite">아이콘 스프라이트</param>
    /// <param name="onClickBuy">구매 콜백</param>
    /// <param name="onClickSell">판매 콜백</param>
    public void Setup(int slotIndex, Sprite iconSprite, Action<int> onClickBuy, Action<int> onClickSell)
    {
        _isSetup = false;

        _slotIndex = slotIndex;
        _onClickBuy = onClickBuy;
        _onClickSell = onClickSell;

        if (_itemIcon != null)
        {
            _itemIcon.sprite = iconSprite;
            _itemIcon.enabled = iconSprite != null;
        }

        if (_buyText != null)
        {
            _buyText.text = "구매";
        }

        if (_sellText != null)
        {
            _sellText.text = "판매";
        }

        BindButtons();

        _isSetup = true;
    }

    /// <summary>
    /// 슬롯 버튼 활성 여부를 설정합니다.
    /// </summary>
    /// <param name="isInteractable">활성 여부</param>
    public void SetInteractable(bool isInteractable)
    {
        if (_buyButton != null)
        {
            _buyButton.interactable = isInteractable;
        }

        if (_sellButton != null)
        {
            _sellButton.interactable = isInteractable;
        }
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    protected override void Awake()
    {
        base.Awake();
    }

    protected override void OnValidate()
    {
        base.OnValidate();
    }
    #endregion
}
