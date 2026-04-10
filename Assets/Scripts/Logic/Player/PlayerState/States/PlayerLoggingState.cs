using UnityEngine;

public class PlayerLoggingState : PlayerOneTime
{
    private const string LOGGING_PARAM = "Logging";

    private readonly int _hashLogging = Animator.StringToHash(LOGGING_PARAM);

    private static string[] _loggingSound =
    {
        Id.Sfx_Player_WoodHit_4,
    };

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public override bool Enter(in PlayerContext context)
    {
        base.Enter(in context);
        context.anim.Play(_hashLogging);
        int index = Random.Range(0, _loggingSound.Length);
        USound.PlaySfx(_loggingSound[index]);
        return true;
    }
    #endregion
}
