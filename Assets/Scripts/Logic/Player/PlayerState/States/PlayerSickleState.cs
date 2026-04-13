using UnityEngine;

public class PlayerSickleState : PlayerOneTime
{
    private const string SICKLE_PARAM = "Sickle";
    private const float CLIP_LENGTH = 0.517f;

    private readonly int _hashSickle = Animator.StringToHash(SICKLE_PARAM);

    private static string[] _sickleSound =
    {
        Id.Sfx_Environment_StepWetGrass_1,
        Id.Sfx_Environment_StepWetGrass_2,
        Id.Sfx_Environment_StepWetGrass_3,
        Id.Sfx_Environment_StepWetGrass_4,
        Id.Sfx_Environment_StepWetGrass_5,
        Id.Sfx_Environment_StepWetGrass_6,
    };
    private static bool _isPlaying;

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public override bool Enter(in PlayerContext context)
    {
        DataManager.Ins.Player.ChangeState(EPlayerState.Sickle);
        base.Enter(in context);
        ApplyAnimationSpeed(context.anim, CLIP_LENGTH, context.duration);
        context.anim.Play(_hashSickle);
        _isPlaying = false;
        return true;
    }

    public override bool Frame(in PlayerContext context)
    {
        // 사운드
        float animHitTime = _nextAnimationEndTime - (CLIP_LENGTH * 0.15f);
        if (!_isPlaying && Time.time > animHitTime)
        {
            _isPlaying = true;
            int index = Random.Range(0, _sickleSound.Length);
            USound.PlaySfx(_sickleSound[index], context.tr);
            USound.PlaySfx(_sickleSound[index], context.tr);
        }
        if (base.Frame(context) == false)
        {
            return false;
        }
        return true;
    }
    #endregion
}
