using UnityEngine;

public class PlayerDrinkingState : PlayerOneTime
{
    private const string DRINKING_PARAM = "Eating";

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
        base.Enter(in context);
        context.anim.Play(_hashDrinking);
        int index = Random.Range(0, _drinkingSound.Length);
        USound.PlaySfx(_drinkingSound[index]);
        return true;
    }
    #endregion
}
