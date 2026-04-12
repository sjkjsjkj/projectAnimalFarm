using UnityEngine;

public class PlayerLoggingState : PlayerOneTime
{
    private const string LOGGING_PARAM = "Logging";
    private const float CLIP_LENGTH = 1f;

    private readonly int _hashLogging = Animator.StringToHash(LOGGING_PARAM);

    private static string[] _loggingSound =
    {
        Id.Sfx_Player_WoodHit_4,
    };

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public override bool Enter(in PlayerContext context)
    {
        DataManager.Ins.Player.ChangeState(EPlayerState.Logging);
        base.Enter(in context);
        ApplyAnimationSpeed(context.anim, CLIP_LENGTH, context.duration);
        context.anim.Play(_hashLogging);
        int index = Random.Range(0, _loggingSound.Length);
        USound.PlaySfx(_loggingSound[index]);
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
