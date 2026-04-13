using UnityEngine;

/// <summary>
/// 플레이어가 일정 거리 이상 멀어지면 연결된 UI를 자동으로 닫습니다.
/// </summary>
public class UIAutoCloseOnDistance : BaseMono
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("자동 닫기 설정")]
    [SerializeField] private float _closeDistance = 3.0f;

    [Header("닫을 UI 대상 (IEscClosable 구현체)")]
    [SerializeField] private BaseMono _targetUiMono;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private IEscClosable _targetUi;
    private Transform _playerTransform;
    private bool _isTracking = false;
    #endregion

    #region ─────────────────────────▶ 외부 메서드 ◀─────────────────────────
    /// <summary>
    /// 인터랙션 시작 시 호출 — 거리 추적을 시작합니다.
    /// </summary>
    public void StartTracking(Transform playerTransform)
    {
        _playerTransform = playerTransform;
        _isTracking = true;
    }

    /// <summary>
    /// UI가 닫힐 때 호출 — 거리 추적을 중단합니다.
    /// </summary>
    public void StopTracking()
    {
        _isTracking = false;
        _playerTransform = null;
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    private void CheckDistance()
    {
        if (_isTracking == false || _playerTransform == null) return;

        float dist = Vector3.Distance(transform.position, _playerTransform.position);
        if (dist >= _closeDistance)
        {
            _targetUi?.CloseUi();
            StopTracking();
        }
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    protected override void Awake()
    {
        base.Awake();

        if (_targetUiMono == null)
        {
            UDebug.Print("UIAutoCloseOnDistance: targetUiMono가 비어 있습니다.", LogType.Warning);
            return;
        }
        if (_targetUiMono.TryGetComponent(out _targetUi) == false)
        {
            UDebug.Print("UIAutoCloseOnDistance: IEscClosable을 구현하지 않은 오브젝트입니다.", LogType.Assert);
        }
    }

    private void Update()
    {
        CheckDistance();
    }
    #endregion
}


#region
/*
public class WorkbenchInteractionObject : BaseMono, IInteractable
{
    [Header("제작대 UI")]
    [SerializeField] private WorkbenchUI _workUI;

    // ★ 추가
    private UIAutoCloseOnDistance _autoClose;

    public void Interact(GameObject player)
    {
        _workUI.SetToggleUI();

        // ★ 열릴 때 추적 시작, 닫힐 때 추적 중단
        if (_workUI.gameObject.activeSelf)
            _autoClose.StartTracking(player.transform);
        else
            _autoClose.StopTracking();
    }

    protected override void Awake()
    {
        base.Awake();
        // ★ 같은 오브젝트에 붙어있거나 GetComponent로 가져오기
        _autoClose = GetComponent<UIAutoCloseOnDistance>();
        StartCoroutine(CoWaitRootLoading());
    }

    // ... 나머지 기존 코드 유지
}



 

*/
#endregion
