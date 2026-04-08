using UnityEngine;

/// <summary>
/// 이벤트를 수신하여 컨텍스트를 빌드하고 상태 머신을 호출합니다.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : BaseMono
{
    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    [SerializeField] private PlayerStateMachine _fsm = new();
    private Rigidbody2D _rb;
    private SpriteRenderer _sprite;
    private Animator _anim;

    private Vector2 _inputMove;
    private bool _inputRun;
    private bool _inputFishing;
    private bool _inputMining;
    private bool _inputLogging;
    private bool _inputDrinking;
    private bool _inputEating;
    private Vector2 _targetPos;
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    private void PlayerMoveHandle(OnPlayerMove ctx)
    {
        _inputMove = ctx.moved;
    }
    private void PlayerRunHandle(OnPlayerRun ctx)
    {
        _inputRun = ctx.isRun;
    }
    private void PlayerFishingHandle(Vector2 taregtPos)
    {
        _inputFishing = true;
        _targetPos = taregtPos;
    }
    private void PlayerMiningHandle(Vector2 taregtPos)
    {
        _inputMining = true;
        _targetPos = taregtPos;
    }
    private void PlayerLoggingHandle(Vector2 taregtPos)
    {
        _inputLogging = true;
        _targetPos = taregtPos;
    }
    private void PlayerDrinkingHandle(Vector2 taregtPos)
    {
        _inputDrinking = true;
        _targetPos = taregtPos;
    }
    private void PlayerEatingHandle(Vector2 taregtPos)
    {
        _inputEating = true;
        _targetPos = taregtPos;
    }
    private void ClearInput()
    {
        _inputFishing = false;
        _inputMining = false;
        _inputLogging = false;
        _inputDrinking = false;
        _inputEating = false;
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    private void FixedUpdate() // 구조체를 작성하여 상태 머신에 전달
    {
        PlayerContext context = new(_rb, transform, _sprite, _anim, _inputMove, _inputRun,
            _inputFishing, _inputMining, _inputLogging, _inputEating, _inputDrinking, _targetPos);
        _fsm.UpdateState(in context);
        ClearInput();
    }

    protected override void Awake()
    {
        base.Awake();
        _rb = UObject.GetComponent<Rigidbody2D>(gameObject);
        _sprite = UObject.GetComponent<SpriteRenderer>(gameObject);
        _anim = UObject.GetComponent<Animator>(gameObject);
        if (_rb == null)
        {
            UDebug.Print($"리지드바디 컴포넌트를 탐색하지 못하여 오브젝트를 비활성화합니다.", LogType.Error);
            UObject.SetActive(gameObject, false);
            return;
        }
        if (_sprite == null)
        {
            UDebug.Print($"스프라이트 컴포넌트를 탐색하지 못하여 오브젝트를 비활성화합니다.", LogType.Error);
            UObject.SetActive(gameObject, false);
            return;
        }
        if (_anim == null)
        {
            UDebug.Print($"애니메이터 컴포넌트를 탐색하지 못하여 오브젝트를 비활성화합니다.", LogType.Error);
            UObject.SetActive(gameObject, false);
            return;
        }
    }

    private void OnEnable()
    {
        EventBus<OnPlayerMove>.Subscribe(PlayerMoveHandle);
        EventBus<OnPlayerRun>.Subscribe(PlayerRunHandle);
    }

    private void OnDisable()
    {
        EventBus<OnPlayerMove>.Unsubscribe(PlayerMoveHandle);
        EventBus<OnPlayerRun>.Unsubscribe(PlayerRunHandle);
    }
    #endregion
}
