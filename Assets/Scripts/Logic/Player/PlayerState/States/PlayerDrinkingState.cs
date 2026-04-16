using UnityEngine;

public class PlayerDrinkingState : PlayerOneTime
{
    private const string DRINKING_PARAM = "Feeding";
    private const float CLIP_LENGTH = 0.517f;

    private readonly int _hashDrinking = Animator.StringToHash(DRINKING_PARAM);

    private static string[] _drinkingSound =
    {
        Id.Sfx_Player_Drink_2,
        Id.Sfx_Player_Drink_3,
        Id.Sfx_Player_Drink_4,
    };

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public override bool Enter(in PlayerContext context)
    {
        DataManager.Ins.Player.ChangeState(EPlayerState.Drinking);
        base.Enter(in context);
        ApplyAnimationSpeed(context.anim, CLIP_LENGTH, context.duration);
        context.anim.Play(_hashDrinking);
        context.anim.speed = 0f;
        int index = Random.Range(0, _drinkingSound.Length);
        USound.PlaySfx(_drinkingSound[index]);
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

    public override void Exit(in PlayerContext context)
    {
        base.Exit(context);
        context.anim.speed = 1f;
    }
    #endregion
}
