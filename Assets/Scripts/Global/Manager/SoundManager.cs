using UnityEngine;

/// <summary>
/// 부팅 시 사운드 테이블을 로드하고 요청받은 소리를 재생하는 사운드 매니저입니다.
/// </summary>
public class SoundManager : GlobalSingleton<SoundManager>
{
    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private bool _isInitialized = false;
    private AudioSource _mainSource;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public override void Initialize()
    {
        if (_isInitialized)
        {
            return;
        }

        // ↑ 필요한 초기화 로직 / 부모 클래스에서 자동 실행
        _isInitialized = true;
    }

    /// <summary>
    /// 메인 카메라에서 사운드를 재생합니다.
    /// </summary>
    public void PlaySfx(string id)
    {
        SoundSO soundSO = null;
        if (TryGetMainSource(out AudioSource mainSource)
            && TryGetSoundSO(id, out soundSO))
        {
            PlaySfx(mainSource, soundSO);
        }
        // 오디오 소스 또는 SO 누락
        else
        {
            UDebug.Print("효과음을 재생하지 못했습니다. " +
                $"(AudioSource = {mainSource}, soundSO = {(soundSO == null ? null : soundSO.name)})");
        }
    }

    /// <summary>
    /// 좌표에서 사운드를 재생합니다.
    /// </summary>
    public void PlaySfx(string id, Vector3 pos)
    {
        SoundSO soundSO = null;
        if (TryGetMainSource(out AudioSource mainSource)
            && TryGetSoundSO(id, out soundSO))
        {
            //PlaySfx(mainSource, soundSO, pos);
        }
        // 오디오 소스 또는 SO 누락
        else
        {
            UDebug.Print("효과음을 재생하지 못했습니다. " +
                $"(AudioSource = {mainSource}, soundSO = {(soundSO == null ? null : soundSO.name)})");
        }
    }

    /// <summary>
    /// 오브젝트에서 사운드를 재생합니다.
    /// </summary>
    public void PlaySfx(string id, Transform tr)
    {
        SoundSO soundSO = null;
        if (TryGetMainSource(out AudioSource mainSource)
            && TryGetSoundSO(id, out soundSO))
        {
            //PlaySfx(mainSource, soundSO, tr);
        }
        // 오디오 소스 또는 SO 누락
        else
        {
            UDebug.Print("효과음을 재생하지 못했습니다. " +
                $"(AudioSource = {mainSource}, soundSO = {(soundSO == null ? null : soundSO.name)})");
        }
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    // 메인 오디오 소스를 갱신합니다.
    private bool TryGetMainSource(out AudioSource source)
    {
        if (_mainSource != null)
        {
            source = _mainSource;
            return true;
        }
        if (UCamera.MainCamera.TryGetComponent(out source))
        {
            _mainSource = source;
            UDebug.Print($"메인 카메라에서 새로운 오디오 소스를 캐싱했습니다.");
            return true;
        }
        UDebug.Print($"메인 카메라에서 오디오 소스를 찾지 못했습니다.", LogType.Assert);
        return false;
    }

    // 효과음 재생
    private void PlaySfx(AudioSource source, SoundSO so)
    {
        source.clip = so.Clip;
        source.volume = so.Volume * GameManager.Ins.Volume.Sfx;
        source.Play();
    }

    // 배경음악 재생
    private void PlayBgm(AudioSource source, SoundSO so)
    {
        source.clip = so.Clip;
        source.volume = so.Volume * GameManager.Ins.Volume.Bgm;
        source.Play();
    }

    // 사운드 SO를 안전하게 가져오기
    private bool TryGetSoundSO(string id, out SoundSO so)
    {
        so = DatabaseManager.Ins.Sound(id);
        return so != null;
    }
    #endregion
}
