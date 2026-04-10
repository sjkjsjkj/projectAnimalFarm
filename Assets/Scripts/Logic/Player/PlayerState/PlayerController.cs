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
    private bool _inputShovel;
    private bool _inputSickle;
    private bool _inputDrinking;
    private bool _inputEating;
    private Vector2 _targetPos;
    private float _duration;
    private bool _isSuccess;
    private bool _isCanceled;
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
    private void PlayerFishingHandle(OnPlayerFishing ctx)
    {
        _inputFishing = true;
        _targetPos = ctx.fishingPointPos;
        _duration = ctx.duration;
        _isSuccess = ctx.isSuccess;
    }
    private void PlayerMiningHandle(OnPlayerMining ctx)
    {
        _inputMining = true;
        _targetPos = ctx.orePos;
        _duration = ctx.duration;
    }
    private void PlayerLoggingHandle(OnPlayerLogging ctx)
    {
        _inputLogging = true;
        _targetPos = ctx.woodPos;
        _duration = ctx.duration;
    }
    private void PlayerShovelHandle(OnPlayerShovel ctx)
    {
        _inputShovel = true;
        _targetPos = ctx.targetPos;
        _duration = ctx.duration;
    }
    private void PlayerSickleHandle(OnPlayerSickle ctx)
    {
        _inputSickle = true;
        _targetPos = ctx.targetPos;
        _duration = ctx.duration;
    }
    private void PlayerDrinkingHandle(OnPlayerDrinking ctx)
    {
        _inputDrinking = true;
        _duration = ctx.drinkingTime;
        _duration = ctx.duration;
    }
    private void PlayerEatingHandle(OnPlayerEating ctx)
    {
        _inputEating = true;
        _duration = ctx.eatingTime;
        _duration = ctx.duration;
    }
    private void PlayerCanceledHandle(OnPlayerCanceled ctx)
    {
        _isCanceled = true;
    }
    private void ClearInput()
    {
        _inputFishing = false;
        _inputMining = false;
        _inputLogging = false;
        _inputShovel = false;
        _inputSickle = false;
        _inputDrinking = false;
        _inputEating = false;
        _isSuccess = false;
        _isCanceled = false;
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    private void FixedUpdate() // 구조체를 작성하여 상태 머신에 전달
    {
        PlayerContext context = new(_rb, transform, _sprite, _anim,
            _inputMove, _inputRun, _inputFishing, _inputMining, _inputLogging,
            _inputShovel, _inputSickle, _inputDrinking, _inputEating,
            _targetPos, _duration, _isSuccess, _isCanceled);
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
        EventBus<OnPlayerFishing>.Subscribe(PlayerFishingHandle);
        EventBus<OnPlayerMining>.Subscribe(PlayerMiningHandle);
        EventBus<OnPlayerLogging>.Subscribe(PlayerLoggingHandle);
        EventBus<OnPlayerSickle>.Subscribe(PlayerSickleHandle);
        EventBus<OnPlayerShovel>.Subscribe(PlayerShovelHandle);
        EventBus<OnPlayerDrinking>.Subscribe(PlayerDrinkingHandle);
        EventBus<OnPlayerEating>.Subscribe(PlayerEatingHandle);
        EventBus<OnPlayerCanceled>.Subscribe(PlayerCanceledHandle);
    }

    private void OnDisable()
    {
        EventBus<OnPlayerMove>.Unsubscribe(PlayerMoveHandle);
        EventBus<OnPlayerRun>.Unsubscribe(PlayerRunHandle);
        EventBus<OnPlayerFishing>.Unsubscribe(PlayerFishingHandle);
        EventBus<OnPlayerMining>.Unsubscribe(PlayerMiningHandle);
        EventBus<OnPlayerLogging>.Unsubscribe(PlayerLoggingHandle);
        EventBus<OnPlayerSickle>.Unsubscribe(PlayerSickleHandle);
        EventBus<OnPlayerShovel>.Unsubscribe(PlayerShovelHandle);
        EventBus<OnPlayerDrinking>.Unsubscribe(PlayerDrinkingHandle);
        EventBus<OnPlayerEating>.Unsubscribe(PlayerEatingHandle);
        EventBus<OnPlayerCanceled>.Unsubscribe(PlayerCanceledHandle);
    }
    #endregion
}
