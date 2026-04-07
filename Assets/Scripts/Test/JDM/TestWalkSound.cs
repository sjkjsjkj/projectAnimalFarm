using UnityEngine;

/// <summary>
/// 플레이어에게 붙여보세요.
/// </summary>
public class TestWalkSound : Frameable
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("걸음 설정")]
    [SerializeField] private float _stepSoundInterval = 0.2f;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private float _nextStepTime;
    private string[] _stepSoundArr;
    private Vector2 _prevPos;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    // 프레임 매니저에게 호출당하는 순서
    public override EPriority Priority => EPriority.Last;

    // 프레임 매니저가 실행할 메서드
    public override void ExecuteFrame()
    {
        if (_prevPos != (Vector2)transform.position)
        {
            if (UMath.TryCooldownEnd(Time.time, ref _nextStepTime, _stepSoundInterval))
            {
                PlayRandomStepSound();
            }
        }
        _prevPos = transform.position;
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    private void PlayRandomStepSound()
    {
        int index = Random.Range(0, _stepSoundArr.Length);
        USound.PlaySfx(_stepSoundArr[index]);
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    private void Start()
    {
        _prevPos = transform.position;
        _stepSoundArr = new string[6];
        _stepSoundArr[0] = Id.Sfx_Environment_StepGrass_1;
        _stepSoundArr[1] = Id.Sfx_Environment_StepGrass_2;
        _stepSoundArr[2] = Id.Sfx_Environment_StepGrass_3;
        _stepSoundArr[3] = Id.Sfx_Environment_StepGrass_4;
        _stepSoundArr[4] = Id.Sfx_Environment_StepGrass_5;
        _stepSoundArr[5] = Id.Sfx_Environment_StepGrass_6;
    }
    #endregion
}
