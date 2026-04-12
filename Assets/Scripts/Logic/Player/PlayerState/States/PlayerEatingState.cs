using UnityEngine;

public class PlayerEatingState : PlayerOneTime
{
    private const string EATING_PARAM = "Eating";
    private const float CLIP_LENGTH = 1f;

    private readonly int _hashEating = Animator.StringToHash(EATING_PARAM);

    private static string[] _eatingSound =
    {
        Id.Sfx_Player_Eat_4,
        Id.Sfx_Player_Eat_5,
    };

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public override bool Enter(in PlayerContext context)
    {
        DataManager.Ins.Player.ChangeState(EPlayerState.Eating);
        base.Enter(in context);
        int index = Random.Range(0, _eatingSound.Length);
        USound.PlaySfx(_eatingSound[index]);
        ApplyAnimationSpeed(context.anim, CLIP_LENGTH, context.duration);
        context.anim.Play(_hashEating);
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
