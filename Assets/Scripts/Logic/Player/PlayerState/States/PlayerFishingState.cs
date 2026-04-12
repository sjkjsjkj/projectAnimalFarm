using UnityEngine;

[System.Serializable]
public class PlayerFishingState : IPlayerState
{
    [Header("낚시 애니메이션 지속시간")]
    [SerializeField] private int _castingWeight;
    [SerializeField] private int _waitWeight;
    [SerializeField] private int _hookedWeight;
    [SerializeField] private int _rollWeight;
    [SerializeField] private int _capturedYesWeight;
    [SerializeField] private int _capturedNoWeight;

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private const string FACING_PARAM = "fFacing";
    private const string PHASE_PARAM = "fFishing";
    private const string FISHING_PARAM = "Fishing";

    private readonly int _hashFacing = Animator.StringToHash(FACING_PARAM);
    private readonly int _hashPhase = Animator.StringToHash(PHASE_PARAM);
    private readonly int _hashFishing = Animator.StringToHash(FISHING_PARAM);

    private float _facingValue;
    private float _totalDuration;
    private float _curPhaseDuration;
    private bool _isSuccess;
    private EFishingState _fishingState;
    private float _nextSoundTime;

    private static string _fishingStartSound = Id.Sfx_Player_FishingStart_1;
    private static string _waterStartSound = Id.Sfx_Player_FishingWater_2;
    private static string[] _hookedSound =
    {
        Id.Sfx_Environment_WaterSplashes_1,
        Id.Sfx_Environment_WaterSplashes_2,
        Id.Sfx_Environment_WaterSplashes_3,
    };
    private static string[] _rollSound =
    {
        Id.Sfx_Environment_WaterSplashes_4,
        Id.Sfx_Environment_WaterSplashes_5,
        Id.Sfx_Environment_WaterSplashes_6,
    };
    private static string[] _capturedYesSound =
    {
        Id.Sfx_Player_FishingWater_5,
        Id.Sfx_Player_FishingWater_8,
    };
    private static string[] _capturedNoSound =
    {
        Id.Sfx_Environment_WaterExit_1,
        Id.Sfx_Environment_WaterExit_2,
    };
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public bool Enter(in PlayerContext context)
    {
        DataManager.Ins.Player.ChangeState(EPlayerState.Fishing);
        _fishingState = EFishingState.Casting;
        _isSuccess = context.isSuccess;
        _totalDuration = context.duration;
        // 방향
        Vector2 playerPos = context.tr.position;
        Vector2 dir = (context.targetPos - playerPos).normalized;
        _facingValue = UPlayer.GetFacingValue(dir);
        // 애니메이션 세팅
        context.anim.Play(_hashFishing);
        context.anim.SetFloat(_hashFacing, _facingValue);
        UPlayer.SetSpriteFacing(context.sprite, dir);
        // 첫 페이즈로 설정
        SetPhase(EFishingState.Casting, _castingWeight, in context);
        // 낚시대 던지기 사운드
        USound.PlaySfx(_fishingStartSound);
        return true;
    }

    public bool Frame(in PlayerContext context)
    {
        context.rb.velocity = Vector2.zero; // 이동 차단
        if (Time.time >= _curPhaseDuration)
        {
            if (!TransitionNextState(in context))
            {
                return false;
            }
        }
        // 사운드 재생
        if (UMath.TryCooldownEnd(Time.time, ref _nextSoundTime, 0.3f))
        {
            if (EFishingState.Hooked == _fishingState)
            {
                int hookedI = Random.Range(0, _hookedSound.Length);
                USound.PlaySfx(_hookedSound[hookedI]);
            }
            else if (EFishingState.Roll == _fishingState)
            {
                int rollI = Random.Range(0, _rollSound.Length);
                USound.PlaySfx(_rollSound[rollI]);
            }
        }
        return true;
    }

    public void Exit(in PlayerContext context) { }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    private void SetPhase(EFishingState nextState, int weight, in PlayerContext ctx)
    {
        float fishingValue = GetFishingValue(nextState);
        float newDuration = WeightToDuration(weight);
        // 새로운 페이즈 설정
        _fishingState = nextState;
        ctx.anim.SetFloat(_hashPhase, fishingValue);
        _curPhaseDuration = Time.time + newDuration;
    }

    // 다음 페이즈로 전환 시도
    private bool TransitionNextState(in PlayerContext ctx)
    {
        switch (_fishingState)
        {
            case EFishingState.Casting:
                SetPhase(EFishingState.Wait, _waitWeight, in ctx);
                USound.PlaySfx(_waterStartSound);
                return true;
            case EFishingState.Wait:
                SetPhase(EFishingState.Hooked, _hookedWeight, in ctx);
                return true;
            case EFishingState.Hooked:
                SetPhase(EFishingState.Roll, _rollWeight, in ctx);
                int roolI = Random.Range(0, _rollSound.Length);
                USound.PlaySfx(_rollSound[roolI]);
                return true;
            case EFishingState.Roll:
                if (_isSuccess)
                {
                    SetPhase(EFishingState.CapturedYes, _capturedYesWeight, in ctx);
                    int capturedI = Random.Range(0, _capturedYesSound.Length);
                    USound.PlaySfx(_capturedYesSound[capturedI]);
                }
                else
                {
                    SetPhase(EFishingState.CapturedNo, _capturedNoWeight, in ctx);
                    int capturedI = Random.Range(0, _capturedNoSound.Length);
                    USound.PlaySfx(_capturedNoSound[capturedI]);
                }
                return true;
            case EFishingState.CapturedYes:
            case EFishingState.CapturedNo:
                return false;
        }
        UDebug.Print($"상태의 비정상 전이 발생 {_fishingState}", LogType.Assert);
        return false;
    }

    // 애니메이터에 사용할 Value값 반환
    private float GetFishingValue(EFishingState state)
    {
        switch (state)
        {
            case EFishingState.Casting:
                return 0f;
            case EFishingState.Wait:
                return 0.2f;
            case EFishingState.Hooked:
                return 0.4f;
            case EFishingState.Roll:
                return 0.6f;
            case EFishingState.CapturedYes:
                return 0.8f;
            case EFishingState.CapturedNo:
                return 1f;
            default:
                UDebug.Print($"존재하지 않는 상태를 받았습니다. {state}", LogType.Assert);
                break;
        }
        return -1f;
    }

    private float WeightToDuration(int weight)
    {
        int total = 0;
        total += _castingWeight;
        total += _waitWeight;
        total += _hookedWeight;
        total += _rollWeight;
        total += _isSuccess ? _capturedYesWeight : _capturedNoWeight;
        if (total <= 0)
        {
            return 0f;
        }
        return _totalDuration * ((float)weight / (float)total);
    }
    #endregion

    private enum EFishingState : byte
    {
        None = 0,
        Casting = 1,
        Wait = 2,
        Hooked = 3,
        Roll = 4,
        CapturedYes = 5,
        CapturedNo = 6,
    }
}
