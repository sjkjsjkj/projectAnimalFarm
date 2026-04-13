using UnityEngine;

public class PlayerCrouchingState : PlayerOneTime
{
    private const string CROUCHING_PARAM = "Crouching";
    private const float CLIP_LENGTH = 0.35f;

    private readonly int _hashCrouching = Animator.StringToHash(CROUCHING_PARAM);

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public override bool Enter(in PlayerContext context)
    {
        DataManager.Ins.Player.ChangeState(EPlayerState.Crouching);
        base.Enter(in context);
        ApplyAnimationSpeed(context.anim, CLIP_LENGTH, context.duration);
        context.anim.Play(_hashCrouching);
        return true;
    }

    public override bool Frame(in PlayerContext context)
    {
        if (base.Frame(context) == false)
        {
            return false;
        }
        return true;
    }
    #endregion
}
