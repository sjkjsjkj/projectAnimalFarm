using UnityEngine;

[System.Serializable]
public class PlayerIdleState : IPlayerState
{
    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private const string ACTION_SPEED_PARAM = "fActionSpeed";
    private const string MOVE_SPEED_PARAM = "fMoveSpeed";
    private const string LOCOMOTION_PARAM = "Locomotion";

    private readonly int _hashActionSpeed = Animator.StringToHash(ACTION_SPEED_PARAM);
    private readonly int _hashSpeed = Animator.StringToHash(MOVE_SPEED_PARAM);
    private readonly int _hashLocomotion = Animator.StringToHash(LOCOMOTION_PARAM);
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public bool Enter(in PlayerContext context)
    {
        DataManager.Ins.Player.ChangeState(EPlayerState.Idle);
        context.anim.SetFloat(_hashActionSpeed, 1f);
        context.rb.velocity = Vector3.zero;
        context.anim.SetFloat(_hashSpeed, 0); // Idle
        context.anim.Play(_hashLocomotion);
        return false;
    }

    public bool Frame(in PlayerContext context)
    {
        context.rb.velocity = Vector3.zero;
        return false;
    }

    public void Exit(in PlayerContext context) { }
    #endregion
}
