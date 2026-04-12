using UnityEngine;

public class PlayerMiningState : PlayerOneTime
{
    private const string MINING_PARAM = "Mining";
    private const float CLIP_LENGTH = 1f;

    private readonly int _hashMining = Animator.StringToHash(MINING_PARAM);

    private static string[] _miningSound =
    {
        Id.Sfx_Player_WoodHit_1,
    };

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public override bool Enter(in PlayerContext context)
    {
        DataManager.Ins.Player.ChangeState(EPlayerState.Mining);
        base.Enter(in context);
        ApplyAnimationSpeed(context.anim, CLIP_LENGTH, context.duration);
        context.anim.Play(_hashMining);
        // 사운드
        int index = Random.Range(0, _miningSound.Length);
        USound.PlaySfx(_miningSound[index], context.tr);
        return true;
    }

    public override bool Frame(in PlayerContext context)
    {
        if(base.Frame(context) == false)
        {
            return false;
        }
        return true;
    }
    #endregion
}
