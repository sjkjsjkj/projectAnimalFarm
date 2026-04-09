using UnityEngine;

/// <summary>
/// 플레이어 상태를 관리하는 상태 머신
/// </summary>
[System.Serializable]
public class PlayerStateMachine
{
    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    [SerializeField] private PlayerIdleState _idle = new();
    [SerializeField] private PlayerWalkState _walk = new();
    [SerializeField] private PlayerRunState _run = new();
    [SerializeField] private PlayerFishingState _fishing = new();
    [SerializeField] private PlayerMiningState _mining = new();
    [SerializeField] private PlayerLoggingState _logging = new();
    [SerializeField] private PlayerDrinkingState _drinking = new();
    [SerializeField] private PlayerEatingState _eating = new();

    private IPlayerState _curState;
    private bool _lockStateTransition;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public IPlayerState CurState => _curState;

    /// <summary>
    /// 컨트롤러에서 호출할 상태 관리 함수
    /// </summary>
    public void UpdateState(in PlayerContext context)
    {
        // 초기 상태일 경우 Idle로 진입
        if (_curState == null)
        {
            _curState = _idle;
            _curState.Enter(in context);
        }
        // 잠금하지 않았을 경우 상태 변경 시도
        if (!_lockStateTransition)
        {
            IPlayerState next = BuildNextState(in context);
            // 상태 전환
            if (next != _curState)
            {
                ChangeState(next, in context);
            }
        }
        // 현재 상태의 Frame 실행
        _lockStateTransition = _curState.Frame(in context);
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    // 규칙에 따라 상태 변경
    private void ChangeState(IPlayerState next, in PlayerContext context)
    {
        _curState.Exit(in context);
        _curState = next;
        _lockStateTransition = _curState.Enter(in context);
    }

    // 다음 상태를 결정
    private IPlayerState BuildNextState(in PlayerContext context)
    {
        if (context.inputLogging) // 벌목 애니메이션 확정
        {
            return _logging;
        }
        if (context.inputMining) // 채광 애니메이션 확정
        {
            return _mining;
        }
        if (context.inputFishing) // 낚시 애니메이션 확정
        {
            return _fishing;
        }
        if (context.inputDrinking) // 음료 애니메이션 확정
        {
            return _drinking;
        }
        if (context.inputEating) // 음식 애니메이션 확정
        {
            return _eating;
        }
        if (context.inputMove != Vector2.zero) // 걷거나 달리기
        {
            return context.inputRun ? _run : _walk;
        }
        // 입력이 없을 경우
        return _idle;
    }
    #endregion
}
