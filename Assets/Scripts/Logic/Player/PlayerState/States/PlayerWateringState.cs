using UnityEngine;

public class PlayerWateringState : PlayerOneTime
{
    private const string WATERING_PARAM = "Watering";
    private const float CLIP_LENGTH = 0.6f;

    private readonly int _hashWatering = Animator.StringToHash(WATERING_PARAM);

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public override bool Enter(in PlayerContext context)
    {
        DataManager.Ins.Player.ChangeState(EPlayerState.Watering);
        base.Enter(in context);
        ApplyAnimationSpeed(context.anim, CLIP_LENGTH, context.duration);
        context.anim.Play(_hashWatering);
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
