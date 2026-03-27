using UnityEngine;

/// <summary>
/// 소리를 재생하고, 재생이 끝나는 스스로 반환되는 객체
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class SoundEmitter : Frameable, IPoolable
{
    private AudioSource _source;
    
    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    // 소리 재생 시작
    public void Play()
    {

    }

    // 초기화
    public void Setup()
    {
        _source = GetComponent<AudioSource>();
        _source.playOnAwake = false;
    }

    // 프레임 매니저에게 호출당하는 순서
    public override EPriority Priority => EPriority.Last;

    // 프레임 매니저가 실행할 메서드
    public override void ExecuteFrame()
    {
        // 아직 재생중
        if (_source.isPlaying)
        {
            return;
        }
        // 재생이 끝났으므로 풀에 스스로 반납
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    private void Check()
    {
        if (_source.isPlaying)
        {

        }
    }
    #endregion
}
