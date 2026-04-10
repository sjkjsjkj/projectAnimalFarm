using UnityEngine;

public class PlayerSickleState : PlayerOneTime
{
    private const string SICKLE_PARAM = "Sickle";

    private readonly int _hashSickle = Animator.StringToHash(SICKLE_PARAM);

    private static string[] _sickleSound =
    {
        Id.Sfx_Player_WoodHit_2,
        Id.Sfx_Player_WoodHit_3,
    };

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public override bool Enter(in PlayerContext context)
    {
        base.Enter(in context);
        context.anim.Play(_hashSickle);
        return true;
    }
    #endregion
}
