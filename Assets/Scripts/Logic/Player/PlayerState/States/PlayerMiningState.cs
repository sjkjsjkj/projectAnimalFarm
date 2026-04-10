using UnityEngine;

public class PlayerMiningState : PlayerOneTime
{
    private const string MINING_PARAM = "Mining";

    private readonly int _hashMining = Animator.StringToHash(MINING_PARAM);

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public override bool Enter(in PlayerContext context)
    {
        base.Enter(in context);
        context.anim.Play(_hashMining);
        return true;
    }
    #endregion
}
