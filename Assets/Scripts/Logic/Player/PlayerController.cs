using UnityEngine;

/// <summary>
/// 플레이어 오브젝트의 이동과 애니메이션을 담당합니다.
/// </summary>
public class PlayerController : BaseMono
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("애니메이터")]
    [SerializeField] private Animator _animator;

    [Header("좌우 반전용")]
    [SerializeField] private SpriteRenderer _bodySr;

    [Header("달리기 목마름")]
    [SerializeField] private float _tickThirstAmount = 0.1f;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private Rigidbody2D _rb;

    private Vector2 _moveInput = Vector2.zero;
    private Vector2 _prevMoveInput = Vector2.zero;

    private bool _isRun = false;
    private EFacing _facing = EFacing.Down;
    private EMoveAxis _lastMoveAxis = EMoveAxis.None;

    private int _isMoveHash;
    private int _isRunHash;
    private int _facingHash;
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    /// <summary>
    /// 필요한 컴포넌트를 캐싱합니다.
    /// </summary>
    private void CacheComponents()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0f;
    }

    /// <summary>
    /// 애니메이터 파라미터 해시를 캐싱합니다.
    /// </summary>
    private void CacheAnimatorHashes()
    {
        _isMoveHash = Animator.StringToHash("IsMove");
        _isRunHash = Animator.StringToHash("IsRun");
        _facingHash = Animator.StringToHash("Facing");
    }

    /// <summary>
    /// 매 프레임 수신되는 이동 입력 이벤트를 처리합니다.
    /// 이전 프레임과 비교하여 새로 누른 축을 기록한 뒤
    /// 방향, 스프라이트, 애니메이션을 즉시 갱신합니다.
    /// </summary>
    /// <param name="eventData">이동 입력 데이터</param>
    private void OnPlayerMoveEvent(OnPlayerMove eventData)
    {
        Vector2 nextMoveInput = eventData.moved;

        bool prevHasHorizontal = Mathf.Abs(_prevMoveInput.x) > K.SMALL_DISTANCE;
        bool prevHasVertical = Mathf.Abs(_prevMoveInput.y) > K.SMALL_DISTANCE;

        bool nextHasHorizontal = Mathf.Abs(nextMoveInput.x) > K.SMALL_DISTANCE;
        bool nextHasVertical = Mathf.Abs(nextMoveInput.y) > K.SMALL_DISTANCE;

        // 새로 눌린 축만 감지
        bool horizontalJustPressed = nextHasHorizontal && !prevHasHorizontal;
        bool verticalJustPressed = nextHasVertical && !prevHasVertical;

        if (horizontalJustPressed && !verticalJustPressed)
        {
            _lastMoveAxis = EMoveAxis.Horizontal;
        }

        else if (!horizontalJustPressed && verticalJustPressed)
        {
            _lastMoveAxis = EMoveAxis.Vertical;
        }

        else if (horizontalJustPressed && verticalJustPressed)
        {
            // 동시에 눌린 경우 → 수평 유지
            _lastMoveAxis = EMoveAxis.Horizontal; 
        }
        // 둘 다 JustPressed 가 아닌 경우 → _lastMoveAxis 유지

        // 입력이 완전히 없으면 초기화
        if (!nextHasHorizontal && !nextHasVertical)
        {
            _lastMoveAxis = EMoveAxis.None;
        }

        _prevMoveInput = _moveInput;
        _moveInput = nextMoveInput;

        UpdateFacing();
        HandleSpriteFlip();
        HandleAnimation();
    }

    /// <summary>
    /// 달리기 입력 이벤트를 받아 현재 달리기 상태를 저장합니다.
    /// </summary>
    /// <param name="eventData">달리기 입력 데이터</param>
    private void OnPlayerRunEvent(OnPlayerRun eventData)
    {
        _isRun = eventData.isRun;
        HandleAnimation();
    }

    /// <summary>
    /// 현재 입력 상태를 기준으로 최종 이동 속도를 계산합니다.
    /// </summary>
    /// <returns>최종 이동 속도</returns>
    private float GetMoveSpeed()
    {
        var provider = DataManager.Ins.Player;
        bool hasMoveInput = _moveInput.sqrMagnitude > K.SMALL_DISTANCE;
        float baseSpeed = provider.CurWalkSpeed;
        // 달리고 있을 경우
        if (_isRun && hasMoveInput && provider.CurThirst >= _tickThirstAmount)
        {
            return baseSpeed * provider.CurRunMultiplier;
        }
        // 걷거나 정지 상태일 경우
        else
        {
            return baseSpeed;
        }
            
    }

    /// <summary>
    /// Rigidbody2D 속도를 갱신하여 플레이어를 이동시킵니다.
    /// 입력이 없을 때는 velocity를 명시적으로 0으로 초기화합니다.
    /// </summary>
    private void HandleMove()
    {
        if (_rb == null)
        {
            return;
        }
        // 입력이 없다고 판단되면 이동 종료
        if(_moveInput.sqrMagnitude < K.SMALL_DISTANCE)
        {
            _rb.velocity = Vector3.zero;
            return;
        }
        // 이동 실행
        Vector2 playerSize = DatabaseManager.Ins.Player(Id.World_Player).Size;
        Vector2 moveDir = _moveInput.normalized;
        Vector2 curPos = transform.position;
        _rb.velocity = TileManager.Ins.Tile.GetValidVelocity
            (curPos, playerSize, moveDir, GetMoveSpeed());
        // 목마름
        if (_isRun)
        {
            DataManager.Ins.Player.ConsumeThirst(_tickThirstAmount);
        }
    }

    /// <summary>
    /// 현재 입력을 바탕으로 바라보는 방향을 계산합니다.
    /// 대각 입력일 때는 마지막으로 새로 눌린 축을 우선합니다.
    /// 입력이 없을 때는 마지막 방향을 유지합니다.
    /// </summary>
    private void UpdateFacing()
    {
        bool hasHorizontal = Mathf.Abs(_moveInput.x) > K.SMALL_DISTANCE;
        bool hasVertical = Mathf.Abs(_moveInput.y) > K.SMALL_DISTANCE;

        if (hasHorizontal == false && hasVertical == false)
        {
            return;
        }

        if (hasHorizontal == true && hasVertical == true)
        {
            _facing = _lastMoveAxis == EMoveAxis.Vertical
                ? (_moveInput.y > 0.0f ? EFacing.Up : EFacing.Down)
                : EFacing.Side;
            return;
        }

        if (hasVertical == true)
        {
            _facing = _moveInput.y > 0.0f ? EFacing.Up : EFacing.Down;
            return;
        }

        _facing = EFacing.Side;
    }

    /// <summary>
    /// Side 방향일 때 좌우 반전을 처리합니다.
    /// </summary>
    private void HandleSpriteFlip()
    {
        if (_bodySr == null)
        {
            return;
        }

        if (_facing != EFacing.Side)
        {
            return;
        }

        if (_moveInput.x < -K.SMALL_DISTANCE)
        {
            _bodySr.flipX = true;
        }

        else if (_moveInput.x > K.SMALL_DISTANCE)
        {
            _bodySr.flipX = false;
        }
    }

    /// <summary>
    /// 현재 입력 상태를 Animator 파라미터에 반영합니다.
    /// </summary>
    private void HandleAnimation()
    {
        if (_animator == null)
        {
            return;
        }

        var provider = DataManager.Ins.Player;
        bool isMove = _moveInput.sqrMagnitude > K.SMALL_DISTANCE;
        bool isRunAnim = isMove == true && _isRun == true && provider.CurThirst >= _tickThirstAmount;

        _animator.SetBool(_isMoveHash, isMove);
        _animator.SetBool(_isRunHash, isRunAnim);
        _animator.SetInteger(_facingHash, (int)_facing);
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    protected override void Awake()
    {
        base.Awake();
        CacheComponents();
        CacheAnimatorHashes();
    }

    private void OnEnable()
    {
        EventBus<OnPlayerMove>.Subscribe(OnPlayerMoveEvent);
        EventBus<OnPlayerRun>.Subscribe(OnPlayerRunEvent);
    }

    private void Start()
    {
        // 데이터 동기화
        var provider = DataManager.Ins.Player;
        if (provider != null)
        {
            transform.position = provider.Position;
        }
        // 초기화
        UpdateFacing();
        HandleSpriteFlip();
        HandleAnimation();
        // 애니메이션 업데이트
        if (_animator != null)
        {
            _animator.Update(0f);
        }
    }

    private void OnDisable()
    {
        EventBus<OnPlayerMove>.Unsubscribe(OnPlayerMoveEvent);
        EventBus<OnPlayerRun>.Unsubscribe(OnPlayerRunEvent);
    }

    private void FixedUpdate()
    {
        HandleMove();
        // 데이터 업데이트
        var provider = DataManager.Ins.Player;
        if(provider != null)
        {
            if (provider.IsLoaded)
            {
                provider.IsLoaded = false;
                transform.position = provider.Position;
            }
            Vector2 curDir = (_moveInput.sqrMagnitude > 0) ? _moveInput.normalized : provider.Direction;
            provider.SetTransform(transform.position, curDir);
        }
    }
    #endregion
}
