using UnityEngine;

[System.Serializable]
public class PlayerRunState : IPlayerState
{
    [Header("걸음 설정")]
    [SerializeField] private float _stepSoundInterval = 0.2f;

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private const string ACTION_SPEED_PARAM = "fActionSpeed";
    private const string MOVE_SPEED_PARAM = "fMoveSpeed";
    private const string FACING_PARAM = "fFacing";
    private const string LOCOMOTION_PARAM = "Locomotion";

    private readonly int _hashActionSpeed = Animator.StringToHash(ACTION_SPEED_PARAM);
    private readonly int _hashSpeed = Animator.StringToHash(MOVE_SPEED_PARAM);
    private readonly int _hashFacing = Animator.StringToHash(FACING_PARAM);
    private readonly int _hashLocomotion = Animator.StringToHash(LOCOMOTION_PARAM);

    private float _nextStepTime;
    private Vector2 _prevPos;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public bool Enter(in PlayerContext context)
    {
        DataManager.Ins.Player.ChangeState(EPlayerState.Run);
        context.anim.SetFloat(_hashActionSpeed, 1f);
        context.anim.SetFloat(_hashSpeed, 1f); // Run
        context.anim.Play(_hashLocomotion);
        _prevPos = context.tr.position; // 기록용
        return false;
    }

    public bool Frame(in PlayerContext context)
    {
        // 기록용
        Vector2 curPos = context.tr.position;
        float movement = Vector2.Distance(curPos, _prevPos);
        OnPlayerRunning.Publish(movement);
        _prevPos = curPos;
        // 변수 작성
        Vector2 pos = context.tr.position;
        Vector2 size = DatabaseManager.Ins.Player(Id.World_Player).Size;
        Vector2 dir = context.inputMove.normalized;
        var player = DataManager.Ins.Player;
        float speed = player.CurWalkSpeed * player.CurRunMultiplier;
        // 실제 속도 적용
        context.rb.velocity = TileManager.Ins.Tile.GetValidVelocity(pos, size, dir, speed);
        // 이동이 있을 경우 방향 설정
        if (dir.sqrMagnitude > 0)
        {
            float facing = UPlayer.GetFacingValue(dir);
            context.anim.SetFloat(_hashFacing, facing);
            UPlayer.SetSpriteFacing(context.sprite, dir);
        }
        // 발자국 소리 재생
        UPlayer.TryPlayStepSound(ref _nextStepTime, _stepSoundInterval, pos);
        return false;
    }

    public void Exit(in PlayerContext context) { }
    #endregion
}
