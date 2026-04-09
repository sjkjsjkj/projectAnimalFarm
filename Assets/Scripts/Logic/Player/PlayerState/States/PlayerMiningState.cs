using UnityEngine;

[System.Serializable]
public class PlayerMiningState : IPlayerState
{
    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private const string MOVE_SPEED_PARAM = "fMoveSpeed";
    private const string FACING_PARAM = "fFacing";
    private const string LOCOMOTION_PARAM = "Locomotion";

    private readonly int _hashSpeed = Animator.StringToHash(MOVE_SPEED_PARAM);
    private readonly int _hashFacing = Animator.StringToHash(FACING_PARAM);
    private readonly int _hashLocomotion = Animator.StringToHash(LOCOMOTION_PARAM);
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public bool Enter(in PlayerContext context)
    {
        DataManager.Ins.Player.ChangeState(EPlayerState.Mining);
        context.anim.SetFloat(_hashSpeed, 0.5f); // Walk
        context.anim.Play(_hashLocomotion);
        return false;
    }

    public bool Frame(in PlayerContext context)
    {
        // 속도 주입
        Vector2 dir = context.inputMove.normalized;
        context.rb.velocity = dir * DataManager.Ins.Player.CurWalkSpeed;
        // 이동이 있을 경우 방향 설정
        if (dir.sqrMagnitude > 0)
        {
            context.anim.SetFloat(_hashFacing, UPlayer.GetFacingValue(dir));
        }
        return false;
    }

    public void Exit(in PlayerContext context) { }
    #endregion
}
