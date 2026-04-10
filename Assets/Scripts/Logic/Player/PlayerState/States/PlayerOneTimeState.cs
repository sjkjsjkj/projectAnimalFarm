using UnityEngine;

[System.Serializable]
public abstract class PlayerOneTime : IPlayerState
{
    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private const string FACING_PARAM = "fFacing";

    private readonly int _hashFacing = Animator.StringToHash(FACING_PARAM);

    private float _nextAnimationEndTime;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public virtual bool Enter(in PlayerContext context)
    {
        DataManager.Ins.Player.ChangeState(EPlayerState.Mining);
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
        // 취소 요청 or 애니메이션 길이 종료
        if (context.isCanceled || Time.time > _nextAnimationEndTime)
        {
            return false;
        }
        return true;
    }

    public virtual void Exit(in PlayerContext context) { }
    #endregion
}
