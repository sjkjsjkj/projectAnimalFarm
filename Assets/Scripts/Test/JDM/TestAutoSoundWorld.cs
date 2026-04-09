using UnityEngine;

/// <summary>
/// 자신에게서 소리 재생
/// </summary>
public class TestAutoSoundWorld : Frameable
{
    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private float _nextPlayTime;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    // 프레임 매니저에게 호출당하는 순서
    public override EPriority Priority => EPriority.Last;

    // 프레임 매니저가 실행할 메서드
    public override void ExecuteFrame()
    {
        if(UMath.TryCooldownEnd(Time.time, ref _nextPlayTime, 0.5f))
        {
            USound.PlaySfx(Id.Sfx_Creature_Brid_Cries_2, transform);
        }
    }
    #endregion
}
