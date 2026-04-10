using UnityEngine;

/// <summary>
/// 플레이어 상호작용 시 공용 UI 창 열기 요청을 발행하는 월드 오브젝트입니다.
/// UIWindowStackManager를 통해 창을 열고, 거리 초과 시 자동으로 닫습니다.
/// </summary>
public class ShopNpcInteractor : BaseMono, IInteractable
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("대상 UI 창")]
    [SerializeField] private EUIWindowId _windowId = EUIWindowId.NpcShop;

    [Header("동작 옵션")]
    [SerializeField] private bool _toggleOnInteract = true;
    [SerializeField] private bool _printTestLog = true;

    [Header("자동 닫기 옵션")]
    [SerializeField] private bool _useAutoCloseByDistance = true;
    [SerializeField] private float _autoCloseDistance = 2.0f;
    #endregion

    #region ─────────────────────────▶ 접근자 ◀─────────────────────────
    #endregion

    #region ─────────────────────────▶ 중첩 타입 ◀─────────────────────────
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private const string OPEN_MESSAGE = "F : 상점 열기";
    private const string CLOSE_MESSAGE = "F : 상점 닫기";
    private const string INVALID_MESSAGE = "F : 상점을 열 수 없음";

    /// <summary>
    /// 현재 이 NPC와 상호작용하여 상점 창을 연 플레이어입니다.
    /// </summary>
    private GameObject _currentPlayer;

    /// <summary>
    /// 거리 기반 자동 닫기 감시 여부입니다.
    /// </summary>
    private bool _isAutoCloseMonitoring;
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    /// <summary>
    /// 대상 창 ID가 유효한지 검사합니다.
    /// </summary>
    /// <param name="shouldPrintLog">실패 시 로그 출력 여부</param>
    private bool HasValidWindowId(bool shouldPrintLog)
    {
        if (_windowId != EUIWindowId.None)
        {
            return true;
        }

        if (shouldPrintLog)
        {
            UDebug.Print("ShopNpcInteractor의 _windowId가 None입니다.", LogType.Assert);
        }

        return false;
    }

    /// <summary>
    /// 현재 대상 창이 열려 있는지 검사합니다.
    /// </summary>
    private bool IsTargetWindowOpen()
    {
        if (UIWindowStackManager.Ins == null)
        {
            return false;
        }

        return UIWindowStackManager.Ins.IsWindowOpen(_windowId);
    }

    /// <summary>
    /// 직렬화 값의 최소 안전값을 보정합니다.
    /// </summary>
    private void ApplySerializedValueSafety()
    {
        _autoCloseDistance = Mathf.Max(0.1f, _autoCloseDistance);
    }

    /// <summary>
    /// 자동 닫기 감시를 시작합니다.
    /// 창이 실제로 열린 경우에만 호출해야 합니다.
    /// </summary>
    /// <param name="player">상호작용한 플레이어</param>
    private void StartAutoCloseMonitoring(GameObject player)
    {
        if (_useAutoCloseByDistance == false)
        {
            return;
        }

        if (player == null)
        {
            return;
        }

        _currentPlayer = player;
        _isAutoCloseMonitoring = true;
    }

    /// <summary>
    /// 자동 닫기 감시를 종료합니다.
    /// </summary>
    private void StopAutoCloseMonitoring()
    {
        _currentPlayer = null;
        _isAutoCloseMonitoring = false;
    }

    /// <summary>
    /// 플레이어와 NPC 사이의 거리를 검사하여 자동 닫기를 처리합니다.
    /// </summary>
    private void HandleAutoCloseByDistance()
    {
        if (_useAutoCloseByDistance == false)
        {
            return;
        }

        if (_isAutoCloseMonitoring == false)
        {
            return;
        }

        // 플레이어 참조가 사라졌으면 감시를 종료합니다.
        if (_currentPlayer == null)
        {
            StopAutoCloseMonitoring();
            return;
        }

        // 창이 이미 닫혔으면 감시를 종료합니다.
        if (IsTargetWindowOpen() == false)
        {
            StopAutoCloseMonitoring();
            return;
        }

        float sqrDistance = ((Vector2)_currentPlayer.transform.position - (Vector2)transform.position).sqrMagnitude;
        float sqrAutoCloseDistance = _autoCloseDistance * _autoCloseDistance;

        if (sqrDistance <= sqrAutoCloseDistance)
        {
            return;
        }

        OnUIWindowCloseRequested.Publish(_windowId);
        PrintTestLog($"거리 초과로 UI 창 닫기 요청 발행 : {_windowId}");
        StopAutoCloseMonitoring();
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
    #endregion

    #region ─────────────────────────▶ 외부 메서드 ◀─────────────────────────
    /// <summary>
    /// 현재 이 NPC와 상호작용 가능한지 검사합니다.
    /// </summary>
    /// <param name="player">상호작용을 시도하는 플레이어 오브젝트</param>
    /// <returns>상호작용 가능 여부</returns>
    public bool CanInteract(GameObject player)
    {
        if (player == null)
        {
            return false;
        }

        return HasValidWindowId(true);
    }

    /// <summary>
    /// 실제 상호작용을 실행합니다.
    /// 토글 모드에서 창이 이미 열려 있으면 닫기만 하고 감시를 시작하지 않습니다.
    /// </summary>
    /// <param name="player">상호작용을 시도한 플레이어 오브젝트</param>
    public void Interact(GameObject player)
    {
        if (player == null)
        {
            return;
        }

        if (HasValidWindowId(true) == false)
        {
            return;
        }

        if (_toggleOnInteract)
        {
            bool wasOpen = IsTargetWindowOpen();

            OnUIWindowToggleRequested.Publish(_windowId);
            PrintTestLog($"UI 창 토글 요청 발행 : {_windowId}");

            if (wasOpen == false)
            {
                StartAutoCloseMonitoring(player);
                return;
            }

            StopAutoCloseMonitoring();
            return;
        }

        OnUIWindowOpenRequested.Publish(_windowId);
        PrintTestLog($"UI 창 열기 요청 발행 : {_windowId}");
        StartAutoCloseMonitoring(player);
    }

    /// <summary>
    /// 상호작용 안내 메시지를 반환합니다.
    /// </summary>
    /// <returns>상호작용 안내 메시지</returns>
    public string GetMessage()
    {
        if (HasValidWindowId(false) == false)
        {
            return INVALID_MESSAGE;
        }

        if (_toggleOnInteract && IsTargetWindowOpen())
        {
            return CLOSE_MESSAGE;
        }

        return OPEN_MESSAGE;
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    protected override void Awake()
    {
        base.Awake();
        ApplySerializedValueSafety();
    }

    protected override void OnValidate()
    {
        base.OnValidate();
        ApplySerializedValueSafety();
    }

    private void Update()
    {
        HandleAutoCloseByDistance();
    }

    private void OnDisable()
    {
        // NPC 오브젝트가 비활성화될 때 열려 있는 창이 있으면 닫고 감시를 종료합니다.
        if (_isAutoCloseMonitoring && IsTargetWindowOpen())
        {
            OnUIWindowCloseRequested.Publish(_windowId);
        }

        StopAutoCloseMonitoring();
    }

    private void Reset()
    {
        // Interactable 레이어가 이미 프로젝트에 준비되어 있다는 전제로 자동 설정만 수행합니다.
        gameObject.layer = LayerMask.NameToLayer("Interactable");
    }
    #endregion
}
