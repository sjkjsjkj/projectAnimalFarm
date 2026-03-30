using UnityEngine;

/// <summary>
/// 요청받은 소리를 재생하는 사운드 매니저입니다.
/// </summary>
public static class SoundManager
{
    private static AudioSource _mainSource;

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    /// <summary>
    /// 메인 카메라에서 사운드를 재생합니다.
    /// </summary>
    public static void PlaySfx(string id)
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
    public static void PlaySfx(string id, Vector3 pos)
    {
        SoundSO soundSO = null;
        if (TryGetMainSource(out AudioSource mainSource)
            && TryGetSoundSO(id, out soundSO))
        {
            PlaySfx(soundSO, pos);
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
    public static void PlaySfx(string id, Transform tr)
    {
        SoundSO soundSO = null;
        if (TryGetMainSource(out AudioSource mainSource)
            && TryGetSoundSO(id, out soundSO))
        {
            PlaySfx(soundSO, tr);
        }
        // 오디오 소스 또는 SO 누락
        else
        {
            UDebug.Print("효과음을 재생하지 못했습니다. " +
                $"(AudioSource = {mainSource}, soundSO = {(soundSO == null ? null : soundSO.name)})");
        }
    }

    /// <summary>
    /// BGM을 변경하고 재생합니다.
    /// </summary>
    /// <param name="id"></param>
    public static void PlayBgm(string id)
    {
        
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    // 메인 오디오 소스를 갱신합니다.
    private static bool TryGetMainSource(out AudioSource source)
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
    private static void PlaySfx(AudioSource source, SoundSO so)
    {
        source.clip = so.Clip;
        source.volume = so.Volume * DataManager.Ins.Volume.Sfx;
        source.Play();
    }

    // 효과음 재생 ─ 좌표
    private static void PlaySfx(SoundSO so, Vector3 pos)
    {
        var emitter = PoolManager.Ins.Spawn<SoundEmitter>(K.NAME_SOUND_EMITTER);
        emitter.Follow(pos);
        AudioSource source = emitter.Source;
        if(source != null)
        {
            PlaySfx(source, so);
        }
    }

    // 효과음 재생 ─ 트랜스폼
    private static void PlaySfx(SoundSO so, Transform tr)
    {
        var emitter = PoolManager.Ins.Spawn<SoundEmitter>(K.NAME_SOUND_EMITTER);
        emitter.Follow(tr);
        AudioSource source = emitter.Source;
        if(source != null)
        {
            PlaySfx(source, so);
        }
    }

    // 배경음악 재생
    private static void PlayBgm(AudioSource source, SoundSO so)
    {
        source.clip = so.Clip;
        source.volume = so.Volume * DataManager.Ins.Volume.Bgm;
        source.Play();
    }

    // 사운드 SO를 안전하게 가져오기
    private static bool TryGetSoundSO(string id, out SoundSO so)
    {
        so = DatabaseManager.Ins.Sound(id);
        return so != null;
    }
    #endregion
}
