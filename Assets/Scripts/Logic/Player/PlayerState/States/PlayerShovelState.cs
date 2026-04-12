using UnityEngine;

public class PlayerShovelState : PlayerOneTime
{
    private const string SHOVEL_PARAM = "Shovel";
    private const float CLIP_LENGTH = 1f;

    private readonly int _hashShovel = Animator.StringToHash(SHOVEL_PARAM);

    private static string[] _shovelSound =
    {
        Id.Sfx_Player_WoodHit_2,
        Id.Sfx_Player_WoodHit_3,
    };

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public override bool Enter(in PlayerContext context)
    {
        DataManager.Ins.Player.ChangeState(EPlayerState.Shovel);
        base.Enter(in context);
        int index = Random.Range(0, _shovelSound.Length);
        USound.PlaySfx(_shovelSound[index]);
        ApplyAnimationSpeed(context.anim, CLIP_LENGTH, context.duration);
        context.anim.Play(_hashShovel);
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
