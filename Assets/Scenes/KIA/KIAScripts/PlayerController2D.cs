using UnityEngine;

/// <summary>
/// 클래스의 설계 의도입니다.
/// </summary>
///
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController2D : BaseMono
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    //[Header("주제")]
    //[SerializeField] private Class _class;
    [Header("이동 속도")]
    [SerializeField] private float _walkSpeed = 1.0f;

    [Header("달리기 속도")]
    [SerializeField] private float _runSpeed = 2.0f;

    [Header("애니메이터")]
    [SerializeField] private Animator _animator;
    [Header("좌우 반전용")]
    [SerializeField] private SpriteRenderer _bodySr;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private const float MOVE_EPSILON = 0.01f;

    private Rigidbody2D _rigidbody2D;
    private Vector2 _moveInput;
    private bool _isRun;
    private EFacing _facing = EFacing.Down;

    private int _isMoveHash;
    private int _isRunHash;
    private int _facingHash;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────

    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    /// <summary>
    /// 필요한 컴포넌트를 캐싱합니다.
    /// </summary>
    private void CacheComponents()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// 애니메이터 파라미터 해시를 캐싱합니다.
    /// 문자열을 매 프레임 직접 넣는 것보다 조금 더 안전하고 가볍습니다.
    /// </summary>
    private void CacheAnimatorHashes()
    {
        _isMoveHash = Animator.StringToHash("IsMove");
        _isRunHash = Animator.StringToHash("IsRun");
        _facingHash = Animator.StringToHash("Facing");
    }

    /// <summary>
    /// 이동 입력 이벤트를 받아 현재 이동값을 저장합니다.
    /// </summary>
    /// <param name="eventData">이동 입력 데이터</param>
    private void OnPlayerMoveEvent(OnPlayerMove eventData)
    {
        _moveInput = eventData.moved;
    }

    /// <summary>
    /// 달리기 입력 이벤트를 받아 현재 달리기 상태를 저장합니다.
    /// </summary>
    /// <param name="eventData">달리기 입력 데이터</param>
    private void OnPlayerRunEvent(OnPlayerRun eventData)
    {
        _isRun = eventData.isRun;
    }

    /// <summary>
    /// 현재 입력 상태를 기준으로 최종 이동 속도를 계산합니다.
    /// </summary>
    /// <returns>걷기 또는 달리기 속도</returns>
    private float GetMoveSpeed()
    {
        bool hasMoveInput = _moveInput.sqrMagnitude > MOVE_EPSILON;

        if (_isRun == true && hasMoveInput == true)
        {
            return _runSpeed;
        }

        return _walkSpeed;
    }

    /// <summary>
    /// Rigidbody2D 속도를 갱신하여 플레이어를 이동시킵니다.
    /// </summary>
    private void HandleMove()
    {
        Vector2 moveDir = _moveInput.normalized;
        float moveSpeed = GetMoveSpeed();

        _rigidbody2D.velocity = moveDir * moveSpeed;
    }

    /// <summary>
    /// 현재 입력을 바탕으로 바라보는 방향을 계산합니다.
    /// 입력이 없을 때는 마지막 방향을 유지합니다.
    /// </summary>
    private void UpdateFacing()
    {
        bool hasMoveInput = _moveInput.sqrMagnitude > MOVE_EPSILON;

        if (hasMoveInput == false)
        {
            return;
        }

        if (Mathf.Abs(_moveInput.y) > Mathf.Abs(_moveInput.x))
        {
            if (_moveInput.y > 0.0f)
            {
                _facing = EFacing.Up;
            }
            else
            {
                _facing = EFacing.Down;
            }
        }
        else
        {
            _facing = EFacing.Side;
        }
    }

    /// <summary>
    /// Side 방향일 때 좌우 반전을 처리합니다.
    /// 오른쪽 입력이면 기본 방향, 왼쪽 입력이면 뒤집습니다.
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

        if (_moveInput.x < -MOVE_EPSILON)
        {
            _bodySr.flipX = true;
        }
        else if (_moveInput.x > MOVE_EPSILON)
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

        bool isMove = _moveInput.sqrMagnitude > MOVE_EPSILON;
        bool isRunAnim = isMove == true && _isRun == true;

        UpdateFacing();
        HandleSpriteFlip();

        _animator.SetBool(_isMoveHash, isMove);
        _animator.SetBool(_isRunHash, isRunAnim);
        _animator.SetInteger(_facingHash, (int)_facing);
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    private void Awake()
    {
        CacheComponents();
        CacheAnimatorHashes();
    }

    private void OnEnable()
    {
        EventBus<OnPlayerMove>.Subscribe(OnPlayerMoveEvent);
        EventBus<OnPlayerRun>.Subscribe(OnPlayerRunEvent);
    }

    private void OnDisable()
    {
        EventBus<OnPlayerMove>.Unsubscribe(OnPlayerMoveEvent);
        EventBus<OnPlayerRun>.Unsubscribe(OnPlayerRunEvent);
    }

    private void Update()
    {
        HandleAnimation();
    }

    private void FixedUpdate()
    {
        HandleMove();
    }
    #endregion

    #region ─────────────────────────▶ 중첩 타입 ◀─────────────────────────
    private enum EFacing
    {
        Down = 0,
        Up = 1,
        Side = 2
    }
    #endregion
}
