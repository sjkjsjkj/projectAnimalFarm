using UnityEngine;

/// <summary>
/// 낮/밤에 따라 애니메이션 변경
/// </summary>
[RequireComponent(typeof(Animator))]
public class FireAnimation : BaseMono
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [SerializeField] private Animator _animator;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    // 해시 캐싱
    private readonly int _hashIdle = Animator.StringToHash("Idle");
    private readonly int _hashOff = Animator.StringToHash("Off");
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    private void TimeChangedHandle(OnTimeChanged ctx)
    {
        UpdateAnimationState(ctx.isDay);
    }

    private void UpdateAnimationState(bool isDay)
    {
        if (_animator == null) return;
        
        if (isDay)
        {
            _animator.Play(_hashOff);
        }
        else
        {
            _animator.Play(_hashIdle);
        }
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    protected override void Awake()
    {
        base.Awake();
        if (_animator == null)
        {
            _animator = GetComponent<Animator>();
        }
        // 씬 전환 시 애니메이션 갱신
        UpdateAnimationState(TimeAndLight.IsDay);
    }

    private void OnEnable()
    {
        EventBus<OnTimeChanged>.Subscribe(TimeChangedHandle);
    }

    private void OnDisable()
    {
        EventBus<OnTimeChanged>.Unsubscribe(TimeChangedHandle);
    }
    #endregion
}
