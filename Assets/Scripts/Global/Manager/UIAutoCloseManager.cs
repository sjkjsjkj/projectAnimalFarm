using UnityEngine;

/// <summary>
/// 플레이어가 인터랙션으로 연 UI를 하나 추적하다가,
/// 일정 거리 이상 멀어지면 자동으로 닫아주는 전역 매니저입니다.
/// 
/// 사용 대상:
/// - 상점
/// - 제작대
/// - 창고
/// - 먹이통
/// - 기타 월드 상호작용 UI
/// 
/// 현재 구조 기준:
/// - ESC 닫기 책임은 EscManager가 담당
/// - 실제 UI 닫기 호출은 IEscClosable.CloseUi() 사용
/// - 이 매니저는 거리 추적만 담당
/// </summary>
public class UIAutoCloseManager : GlobalSingleton<UIAutoCloseManager>
{
    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    /// <summary>
    /// 초기화 여부입니다.
    /// </summary>
    private bool _isInitialized = false;

    /// <summary>
    /// 현재 추적 중인 플레이어 Transform입니다.
    /// </summary>
    private Transform _playerTransform;

    /// <summary>
    /// 거리 계산 기준이 되는 월드 오브젝트 Transform입니다.
    /// 보통 NPC, 제작대, 창고 등 상호작용 오브젝트입니다.
    /// </summary>
    private Transform _originTransform;

    /// <summary>
    /// 실제로 닫기 호출을 보낼 대상 UI입니다.
    /// </summary>
    private IEscClosable _targetUi;

    /// <summary>
    /// 실제 열림 여부를 확인할 UI 루트입니다.
    /// </summary>
    private GameObject _targetWindowRoot;

    /// <summary>
    /// 자동 닫기 거리입니다.
    /// </summary>
    private float _closeDistance = 3.0f;

    /// <summary>
    /// 2D 거리 계산 사용 여부입니다.
    /// </summary>
    private bool _use2DDistance = true;

    /// <summary>
    /// 현재 추적 중인지 여부입니다.
    /// </summary>
    private bool _isTracking = false;

    /// <summary>
    /// 디버그 로그 출력 여부입니다.
    /// </summary>
    private bool _printLog = false;

    /// <summary>
    /// 디버그용 대상 이름입니다.
    /// </summary>
    private string _debugTargetName = string.Empty;
    #endregion

    #region ─────────────────────────▶ 접근자 ◀─────────────────────────
    /// <summary>
    /// 현재 자동 닫기 추적 중인지 여부입니다.
    /// </summary>
    public bool IsTracking => _isTracking;
    #endregion

    #region ─────────────────────────▶ 외부 메서드 ◀─────────────────────────
    /// <summary>
    /// 전역 매니저 초기화를 수행합니다.
    /// </summary>
    public override void Initialize()
    {
        if (_isInitialized)
        {
            return;
        }

        _isInitialized = true;
    }

    /// <summary>
    /// 자동 닫기 추적을 시작합니다.
    /// 기존 추적 대상이 있으면 새 대상 정보로 덮어씁니다.
    /// </summary>
    /// <param name="playerTransform">상호작용한 플레이어 Transform</param>
    /// <param name="originTransform">거리 계산 기준이 되는 월드 오브젝트 Transform</param>
    /// <param name="targetUiMono">IEscClosable을 구현한 UI 스크립트가 붙은 오브젝트</param>
    /// <param name="targetWindowRoot">실제로 켜지고 꺼지는 UI 루트</param>
    /// <param name="closeDistance">자동 닫기 거리</param>
    /// <param name="use2DDistance">2D 거리 계산 여부</param>
    /// <param name="printLog">디버그 로그 출력 여부</param>
    /// <param name="debugTargetName">디버그용 대상 이름</param>
    public void StartTracking(
        Transform playerTransform,
        Transform originTransform,
        BaseMono targetUiMono,
        GameObject targetWindowRoot,
        float closeDistance = 3.0f,
        bool use2DDistance = true,
        bool printLog = false,
        string debugTargetName = "")
    {
        if (playerTransform == null)
        {
            UDebug.Print("UIAutoCloseManager: playerTransform이 비어 있습니다.", LogType.Warning);
            return;
        }

        if (originTransform == null)
        {
            UDebug.Print("UIAutoCloseManager: originTransform이 비어 있습니다.", LogType.Warning);
            return;
        }

        if (targetUiMono == null)
        {
            UDebug.Print("UIAutoCloseManager: targetUiMono가 비어 있습니다.", LogType.Warning);
            return;
        }

        if (targetUiMono.TryGetComponent(out IEscClosable targetUi) == false)
        {
            UDebug.Print("UIAutoCloseManager: targetUiMono가 IEscClosable을 구현하지 않았습니다.", LogType.Assert);
            return;
        }

        if (targetWindowRoot == null)
        {
            UDebug.Print("UIAutoCloseManager: targetWindowRoot가 비어 있습니다.", LogType.Warning);
            return;
        }

        _playerTransform = playerTransform;
        _originTransform = originTransform;
        _targetUi = targetUi;
        _targetWindowRoot = targetWindowRoot;
        _closeDistance = Mathf.Max(0.1f, closeDistance);
        _use2DDistance = use2DDistance;
        _printLog = printLog;
        _debugTargetName = debugTargetName;
        _isTracking = true;

        if (_printLog)
        {
            UDebug.Print($"UIAutoCloseManager: 추적 시작 ({_debugTargetName})");
        }
    }

    /// <summary>
    /// 자동 닫기 추적을 중단합니다.
    /// </summary>
    public void StopTracking()
    {
        if (_printLog)
        {
            UDebug.Print($"UIAutoCloseManager: 추적 종료 ({_debugTargetName})");
        }

        _playerTransform = null;
        _originTransform = null;
        _targetUi = null;
        _targetWindowRoot = null;
        _closeDistance = 3.0f;
        _use2DDistance = true;
        _printLog = false;
        _debugTargetName = string.Empty;
        _isTracking = false;
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    /// <summary>
    /// 현재 대상 UI가 열려 있는지 검사합니다.
    /// </summary>
    /// <returns>열림 여부</returns>
    private bool IsTargetUiOpen()
    {
        if (_targetWindowRoot == null)
        {
            return false;
        }

        return _targetWindowRoot.activeInHierarchy;
    }

    /// <summary>
    /// 현재 플레이어와 기준점 사이 거리 제곱값을 계산합니다.
    /// </summary>
    /// <returns>거리 제곱값</returns>
    private float GetCurrentSqrDistance()
    {
        if (_use2DDistance)
        {
            Vector2 originPos = _originTransform.position;
            Vector2 playerPos = _playerTransform.position;
            return (originPos - playerPos).sqrMagnitude;
        }

        return (_originTransform.position - _playerTransform.position).sqrMagnitude;
    }

    /// <summary>
    /// 현재 추적 상태를 검사하고, 필요하면 자동 닫기를 수행합니다.
    /// </summary>
    private void CheckDistance()
    {
        if (_isTracking == false)
        {
            return;
        }

        if (_playerTransform == null || _originTransform == null || _targetUi == null)
        {
            StopTracking();
            return;
        }

        // 이미 닫힌 UI는 더 이상 추적할 필요가 없습니다.
        if (IsTargetUiOpen() == false)
        {
            StopTracking();
            return;
        }

        float currentSqrDistance = GetCurrentSqrDistance();
        float closeSqrDistance = _closeDistance * _closeDistance;

        if (currentSqrDistance < closeSqrDistance)
        {
            return;
        }

        if (_printLog)
        {
            UDebug.Print($"UIAutoCloseManager: 거리 초과로 UI 자동 닫기 ({_debugTargetName})");
        }

        _targetUi.CloseUi();
        StopTracking();
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    private void Update()
    {
        CheckDistance();
    }
    #endregion
}
