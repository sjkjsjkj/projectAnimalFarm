using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 상점 UI의 표시, 슬롯 생성, 닫기 버튼 연결을 담당합니다.
/// UIShopView 오브젝트 자체를 표시 루트로 사용하되,
/// 부모 윈도우 루트도 함께 제어할 수 있는 구조입니다.
/// </summary>
public class UIShopView : BaseMono, IShopUI
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("상점 표시 루트")]
    [SerializeField] private GameObject _viewRoot;

    [Header("상점 윈도우 루트")]
    [SerializeField] private GameObject _windowRoot;

    [Header("상점 헤더")]
    [SerializeField] private Button _closeButton;

    [Header("상점 슬롯")]
    [SerializeField] private RectTransform _slotContainer;
    [SerializeField] private UIShopSlotItem _shopSlotItemPrefab;
    #endregion

    #region ─────────────────────────▶ 접근자 ◀─────────────────────────
    /// <summary>
    /// 생성된 런타임 슬롯 목록입니다.
    /// </summary>
    public IReadOnlyList<UIShopSlotItem> Slots => _runtimeSlots;

    /// <summary>
    /// 현재 상점 UI 표시 여부입니다.
    /// activeInHierarchy를 사용해 부모 비활성 상태까지 포함해 검사합니다.
    /// </summary>
    public bool IsVisible => _viewRoot != null && _viewRoot.activeInHierarchy;
    #endregion

    #region ─────────────────────────▶ 중첩 타입 ◀─────────────────────────
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    /// <summary>
    /// 런타임에 생성한 슬롯 목록입니다.
    /// </summary>
    private readonly List<UIShopSlotItem> _runtimeSlots = new();

    /// <summary>
    /// 닫기 버튼 클릭 시 호출할 외부 콜백입니다.
    /// </summary>
    private Action _onClickClose;

    public event Action<string, int, int> OnBuyButtonPressed;
    public event Action<string, int, int> OnSellButtonPressed;
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    /// <summary>
    /// 닫기 버튼 이벤트를 연결합니다.
    /// RemoveListener 후 AddListener로 중복 등록을 방지합니다.
    /// </summary>
    private void BindCloseButton()
    {
        if (_closeButton == null)
        {
            return;
        }

        _closeButton.onClick.RemoveListener(HandleClickClose);
        _closeButton.onClick.AddListener(HandleClickClose);
    }

    /// <summary>
    /// 닫기 버튼 클릭을 처리합니다.
    /// </summary>
    private void HandleClickClose()
    {
        _onClickClose?.Invoke();
    }

    /// <summary>
    /// 슬롯 컨테이너 레이아웃을 즉시 갱신합니다.
    /// </summary>
    private void RefreshSlotLayout()
    {
        if (_slotContainer == null)
        {
            return;
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(_slotContainer);
    }

    /// <summary>
    /// 슬롯 컨테이너 아래의 상점 슬롯들을 모두 제거합니다.
    /// UIShopSlotItem 컴포넌트가 있는 자식만 제거하여
    /// 레이아웃 헤더 등 다른 자식에 영향을 주지 않습니다.
    /// </summary>
    private void ClearSlotContainerChildren()
    {
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

            if (child.TryGetComponent(out UIShopSlotItem slotItem))
            {
                Destroy(slotItem.gameObject);
            }
        }
    }
    #endregion

    #region ─────────────────────────▶ 외부 메서드 ◀─────────────────────────
    /// <summary>
    /// 상점 UI를 초기화합니다.
    /// </summary>
    /// <param name="onClickClose">닫기 버튼 콜백</param>
    public void Initialize(Action onClickClose)
    {
        _onClickClose = onClickClose;
        BindCloseButton();
    }

    /// <summary>
    /// 슬롯 프리팹을 개수만큼 생성합니다.
    /// </summary>
    /// <param name="slotCount">생성할 슬롯 개수</param>
    /// <param name="defaultIcon">기본 아이콘</param>
    /// <param name="onClickBuy">구매 콜백</param>
    /// <param name="onClickSell">판매 콜백</param>
    public void BuildSlots(int slotCount, Sprite defaultIcon, Action<int> onClickBuy, Action<int> onClickSell)
    {
        ClearSlots();

        if (_slotContainer == null || _shopSlotItemPrefab == null)
        {
            UDebug.Print("UIShopView 슬롯 참조가 비어 있습니다.", LogType.Assert);
            return;
        }

        int safeSlotCount = Mathf.Max(1, slotCount);

        for (int i = 0; i < safeSlotCount; ++i)
        {
            UIShopSlotItem slotItem = Instantiate(_shopSlotItemPrefab, _slotContainer);
            if (slotItem == null)
            {
                continue;
            }

            slotItem.name = $"ShopSlotItem_{i + 1:00}";
            slotItem.Setup(i, defaultIcon, onClickBuy, onClickSell,0,0);
            _runtimeSlots.Add(slotItem);
        }

        RefreshSlotLayout();
    }
    /// <summary>
    /// 생성된 슬롯들을 모두 삭제합니다.
    /// </summary>
    public void ClearSlots()
    {
        ClearSlotContainerChildren();
        _runtimeSlots.Clear();
        RefreshSlotLayout();
    }

    /// <summary>
    /// 상점 UI 표시 여부를 설정합니다.
    /// 윈도우 루트와 뷰 루트가 다른 경우 함께 제어합니다.
    /// </summary>
    /// <param name="isVisible">표시 여부</param>
    public void SetVisible(bool isVisible)
    {
        if (_windowRoot != null)
        {
            _windowRoot.SetActive(isVisible);
        }

        if (_viewRoot != null && _viewRoot != _windowRoot)
        {
            _viewRoot.SetActive(isVisible);
        }
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    protected override void Awake()
    {
        base.Awake();

        if (_viewRoot == null)
        {
            _viewRoot = gameObject;
        }

        if (_windowRoot == null)
        {
            _windowRoot = transform.parent != null ? transform.parent.gameObject : gameObject;
        }
    }

    protected override void OnValidate()
    {
        base.OnValidate();

        if (_viewRoot == null)
        {
            _viewRoot = gameObject;
        }

        if (_windowRoot == null)
        {
            _windowRoot = transform.parent != null ? transform.parent.gameObject : gameObject;
        }
    }

    public void BuySuccessHandle(string successMsg)
    {
        throw new NotImplementedException();
    }

    public void BuyFailureHandle(string failMsg)
    {
        throw new NotImplementedException();
    }

    public void SellSuccessHandle(string successMsg)
    {
        throw new NotImplementedException();
    }

    public void SellFailureHandle(string failMsg)
    {
        throw new NotImplementedException();
    }
    #endregion
}
