using UnityEngine;

public class PlayerShovelState : PlayerOneTime
{
    private const string SHOVEL_PARAM = "Shovel";

    private readonly int _hashShovel = Animator.StringToHash(SHOVEL_PARAM);

    private static string[] _shovelSound =
    {
        Id.Sfx_Player_WoodHit_2,
        Id.Sfx_Player_WoodHit_3,
    };

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public override bool Enter(in PlayerContext context)
    {
        base.Enter(in context);
        int index = Random.Range(0, _shovelSound.Length);
        USound.PlaySfx(_shovelSound[index]);
        context.anim.Play(_hashShovel);
        return true;
    }
    #endregion
}
