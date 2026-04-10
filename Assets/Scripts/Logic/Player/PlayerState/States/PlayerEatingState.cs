using UnityEngine;

public class PlayerEatingState : PlayerOneTime
{
    private const string EATING_PARAM = "Eating";

    private readonly int _hashEating = Animator.StringToHash(EATING_PARAM);

    private static string[] _eatingSound =
    {
        Id.Sfx_Player_Eat_4,
        Id.Sfx_Player_Eat_5,
    };

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public override bool Enter(in PlayerContext context)
    {
        base.Enter(in context);
        int index = Random.Range(0, _eatingSound.Length);
        USound.PlaySfx(_eatingSound[index]);
        context.anim.Play(_hashEating);
        return true;
    }
    #endregion
}
