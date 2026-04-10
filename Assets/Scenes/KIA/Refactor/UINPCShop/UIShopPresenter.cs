using UnityEngine;

/// <summary>
/// 상점 UI 입력을 받아 로직에 전달하고,
/// 외부 상호작용으로 창을 열고 닫을 수 있게 관리하는 프리젠터입니다.
/// IUIWindow를 구현하여 UIWindowStackManager의 Esc 닫기 스택에 통합됩니다.
/// </summary>
public class UIShopPresenter : BaseMono, IUIWindow
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("상점 식별자")]
    [SerializeField] private string _shopId = "Shop_Npc_01";

    [Header("임시 슬롯 설정")]
    [SerializeField] private Sprite _defaultIcon;
    [SerializeField] private int _slotCount = 6;

    [Header("테스트 옵션")]
    [SerializeField] private bool _useTestLogic = true;
    [SerializeField] private bool _printTestLog = true;
    [SerializeField] private bool _openOnStart = false;

    [Header("창 옵션")]
    [SerializeField] private bool _canCloseWithEsc = true;

    [Header("피드백 설정")]
    [SerializeField] private float _feedbackDuration = 1.5f;
    #endregion

    #region ─────────────────────────▶ 접근자 ◀─────────────────────────
    /// <summary>
    /// 현재 상점 창 표시 여부입니다.
    /// </summary>
    public bool IsOpen => _shopView != null && _shopView.IsVisible;

    /// <summary>
    /// Esc로 닫을 수 있는지 여부입니다.
    /// </summary>
    public bool CanCloseWithEsc => _canCloseWithEsc;
    #endregion

    #region ─────────────────────────▶ 중첩 타입 ◀─────────────────────────
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private const int MIN_SLOT_COUNT = 1;
    private const float MIN_FEEDBACK_DURATION = 0.1f;

    /// <summary>
    /// 같은 오브젝트 또는 자식에 붙은 상점 뷰입니다.
    /// </summary>
    private UIShopView _shopView;

    /// <summary>
    /// 상점 거래 로직입니다.
    /// </summary>
    private ShopLogic _shopLogic;

    /// <summary>
    /// 초기화 완료 여부입니다.
    /// </summary>
    private bool _isInitialized;
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    /// <summary>
    /// 직렬화 값의 최소 안전값을 보정합니다.
    /// </summary>
    private void ApplySerializedValueSafety()
    {
        _slotCount = Mathf.Max(MIN_SLOT_COUNT, _slotCount);
        _feedbackDuration = Mathf.Max(MIN_FEEDBACK_DURATION, _feedbackDuration);
    }

    /// <summary>
    /// 필요한 컴포넌트 참조를 확보합니다.
    /// 같은 오브젝트 우선, 없으면 비활성 자식까지 탐색합니다.
    /// </summary>
    private void EnsureReferences()
    {
        if (_shopView == null)
        {
            _shopView = GetComponent<UIShopView>();
        }

        if (_shopView == null)
        {
            _shopView = GetComponentInChildren<UIShopView>(true);
        }
    }

    /// <summary>
    /// 상점 뷰 참조가 유효한지 검사합니다.
    /// </summary>
    /// <param name="shouldPrintLog">실패 시 로그 출력 여부</param>
    /// <returns>유효 여부</returns>
    private bool HasValidView(bool shouldPrintLog)
    {
        EnsureReferences();

        if (_shopView != null)
        {
            return true;
        }

        if (shouldPrintLog)
        {
            UDebug.Print("UIShopPresenter가 UIShopView를 찾지 못했습니다.", LogType.Assert);
        }

        return false;
    }

    /// <summary>
    /// 상점 로직이 초기화되어 있는지 검사합니다.
    /// </summary>
    /// <param name="shouldPrintLog">실패 시 로그 출력 여부</param>
    /// <returns>유효 여부</returns>
    private bool HasValidLogic(bool shouldPrintLog)
    {
        if (_shopLogic != null)
        {
            return true;
        }

        if (shouldPrintLog)
        {
            UDebug.Print("UIShopPresenter의 ShopLogic이 초기화되지 않았습니다.", LogType.Assert);
        }

        return false;
    }

    /// <summary>
    /// 테스트용 로그를 출력합니다.
    /// </summary>
    /// <param name="message">출력할 메시지</param>
    private void PrintTestLog(string message)
    {
        if (_printTestLog == false)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        UDebug.Print(message);
    }

    /// <summary>
    /// 상점 슬롯들을 생성합니다.
    /// </summary>
    private void BuildShopSlots()
    {
        if (HasValidView(true) == false)
        {
            return;
        }

        _shopView.BuildSlots(_slotCount, _defaultIcon, HandleClickBuy, HandleClickSell);
    }

    /// <summary>
    /// 구매 버튼 클릭을 처리합니다.
    /// </summary>
    /// <param name="slotIndex">클릭한 슬롯 번호</param>
    private void HandleClickBuy(int slotIndex)
    {
        if (HasValidLogic(true) == false)
        {
            return;
        }

        PlayClickSound();

        if (_shopLogic.TryBuy(slotIndex, out string message))
        {
            PlaySuccessSound();
            ShowFeedback(message, EFeedbackMessageType.Success);
            return;
        }

        PlayErrorSound();
        ShowFeedback(message, EFeedbackMessageType.Failure);
    }

    /// <summary>
    /// 판매 버튼 클릭을 처리합니다.
    /// </summary>
    /// <param name="slotIndex">클릭한 슬롯 번호</param>
    private void HandleClickSell(int slotIndex)
    {
        if (HasValidLogic(true) == false)
        {
            return;
        }

        PlayClickSound();

        if (_shopLogic.TrySell(slotIndex, out string message))
        {
            PlaySuccessSound();
            ShowFeedback(message, EFeedbackMessageType.Success);
            return;
        }

        PlayErrorSound();
        ShowFeedback(message, EFeedbackMessageType.Failure);
    }

    /// <summary>
    /// 닫기 버튼 클릭을 처리합니다.
    /// </summary>
    private void HandleClickClose()
    {
        PlayClickSound();
        CloseShop();
    }

    /// <summary>
    /// UIWindowStackManager에 현재 창을 등록합니다.
    /// </summary>
    private void RegisterToStackManager()
    {
        if (UIWindowStackManager.Ins == null)
        {
            return;
        }

        UIWindowStackManager.Ins.RegisterWindow(this);
    }

    /// <summary>
    /// UIWindowStackManager에서 현재 창을 제거합니다.
    /// </summary>
    private void UnregisterFromStackManager()
    {
        if (UIWindowStackManager.Ins == null)
        {
            return;
        }

        UIWindowStackManager.Ins.UnregisterWindow(this);
    }

    /// <summary>
    /// 클릭 사운드 호출 자리입니다.
    /// </summary>
    private void PlayClickSound()
    {
        // TODO: UI 클릭 사운드 이벤트 발행
    }

    /// <summary>
    /// 거래 성공 사운드 호출 자리입니다.
    /// </summary>
    private void PlaySuccessSound()
    {
        // TODO: 거래 성공 사운드 이벤트 발행
    }

    /// <summary>
    /// 거래 실패 사운드 호출 자리입니다.
    /// </summary>
    private void PlayErrorSound()
    {
        // TODO: 거래 실패 사운드 이벤트 발행
    }

    /// <summary>
    /// 공용 피드백 메시지 표시를 요청합니다.
    /// </summary>
    /// <param name="message">출력할 메시지</param>
    /// <param name="messageType">메시지 타입</param>
    private void ShowFeedback(string message, EFeedbackMessageType messageType)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        OnFeedbackMessageRequested.Publish(message, messageType, _feedbackDuration);
    }
    #endregion

    #region ─────────────────────────▶ 외부 메서드 ◀─────────────────────────
    /// <summary>
    /// 상점 UI를 초기화합니다.
    /// HasValidView 실패 시 _isInitialized를 true로 바꾸지 않아 재시도가 가능합니다.
    /// </summary>
    public void Initialize()
    {
        if (_isInitialized)
        {
            return;
        }

        ApplySerializedValueSafety();

        if (HasValidView(true) == false)
        {
            return;
        }

        _shopLogic = new ShopLogic(_slotCount, _useTestLogic);
        _shopView.Initialize(HandleClickClose);
        BuildShopSlots();

        _isInitialized = true;
        _shopView.SetVisible(false);
    }

    /// <summary>
    /// 상점 창을 엽니다.
    /// IUIWindow 구현입니다.
    /// </summary>
    public void Open()
    {
        OpenShop();
    }

    /// <summary>
    /// 상점 창을 닫습니다.
    /// IUIWindow 구현입니다.
    /// </summary>
    public void Close()
    {
        CloseShop();
    }

    /// <summary>
    /// 상점 창을 엽니다.
    /// 아직 초기화되지 않았다면 먼저 초기화합니다.
    /// </summary>
    public void OpenShop()
    {
        if (_isInitialized == false)
        {
            Initialize();
        }

        if (HasValidView(true) == false)
        {
            return;
        }

        if (IsOpen)
        {
            RegisterToStackManager();
            return;
        }

        _shopView.SetVisible(true);
        RegisterToStackManager();

        PrintTestLog($"UIShopPresenter.OpenShop : {_shopId}");
    }

    /// <summary>
    /// 상점 창을 닫습니다.
    /// </summary>
    public void CloseShop()
    {
        if (HasValidView(false) == false)
        {
            UnregisterFromStackManager();
            return;
        }

        if (IsOpen == false)
        {
            UnregisterFromStackManager();
            return;
        }

        _shopView.SetVisible(false);
        UnregisterFromStackManager();

        PrintTestLog($"UIShopPresenter.CloseShop : {_shopId}");
    }

    /// <summary>
    /// 상점 창 표시 상태를 토글합니다.
    /// </summary>
    public void ToggleShop()
    {
        if (_isInitialized == false)
        {
            Initialize();
        }

        if (HasValidView(true) == false)
        {
            return;
        }

        if (IsOpen)
        {
            CloseShop();
            return;
        }

        OpenShop();
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    protected override void Awake()
    {
        base.Awake();
        EnsureReferences();
        ApplySerializedValueSafety();
    }

    protected override void OnValidate()
    {
        base.OnValidate();
        EnsureReferences();
        ApplySerializedValueSafety();
    }

    private void Start()
    {
        Initialize();

        if (_openOnStart)
        {
            OpenShop();
        }
    }

    private void OnDisable()
    {
        UnregisterFromStackManager();
    }

    private void OnDestroy()
    {
        UnregisterFromStackManager();
    }
    #endregion
}
