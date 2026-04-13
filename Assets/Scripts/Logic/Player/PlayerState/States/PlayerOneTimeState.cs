using UnityEngine;

[System.Serializable]
public abstract class PlayerOneTime : IPlayerState
{
    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    protected const string FACING_PARAM = "fFacing";
    protected const string ACTION_SPEED_PARAM = "fActionSpeed";

    protected readonly int _hashFacing = Animator.StringToHash(FACING_PARAM);
    protected readonly int _hashActionSpeed = Animator.StringToHash(ACTION_SPEED_PARAM);

    protected float _nextAnimationEndTime;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public virtual bool Enter(in PlayerContext context)
    {
        // 방향
        Vector2 playerPos = context.tr.position;
        Vector2 dir = (context.targetPos - playerPos).normalized;
        // 애니메이션
        context.anim.SetFloat(_hashFacing, UPlayer.GetFacingValue(dir));
        _nextAnimationEndTime = Time.time + context.duration;
        return true;
    }

    public virtual bool Frame(in PlayerContext context)
    {
        context.rb.velocity = Vector2.zero;
        // 애니메이션 길이 종료
        if (Time.time > _nextAnimationEndTime)
        {
            return false;
        }
        return true;
    }

    public virtual void Exit(in PlayerContext context) { }
    #endregion

    protected void ApplyAnimationSpeed(Animator anim, float originalClipLength, float targetDuration)
    {
        float speedMultiplier = 1f;
        if(targetDuration > K.SMALL_DISTANCE)
        {
            speedMultiplier = originalClipLength / targetDuration;
        }
        anim.SetFloat(_hashActionSpeed, speedMultiplier);
    }
}
