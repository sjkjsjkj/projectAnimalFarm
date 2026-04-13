using UnityEngine;

public class PlayerShovelState : PlayerOneTime
{
    private const string SHOVEL_PARAM = "Shovel";
    private const float CLIP_LENGTH = 0.433f;

    private readonly int _hashShovel = Animator.StringToHash(SHOVEL_PARAM);

    private static string[] _shovelSound =
    {
        Id.Sfx_Player_WoodHit_2,
        Id.Sfx_Player_WoodHit_3,
    };
    private static bool _isPlaying;

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public override bool Enter(in PlayerContext context)
    {
        DataManager.Ins.Player.ChangeState(EPlayerState.Shovel);
        base.Enter(in context);
        ApplyAnimationSpeed(context.anim, CLIP_LENGTH, context.duration);
        context.anim.Play(_hashShovel);
        _isPlaying = false;
        return true;
    }

    public override bool Frame(in PlayerContext context)
    {
        // 사운드
        float animHitTime = _nextAnimationEndTime - (CLIP_LENGTH * 1f);
        if (!_isPlaying && Time.time > animHitTime)
        {
            _isPlaying = true;
            int index = Random.Range(0, _shovelSound.Length);
            USound.PlaySfx(_shovelSound[index]);
        }
        if (base.Frame(context) == false)
        {
            return false;
        }
        return true;
    }
    #endregion
}
