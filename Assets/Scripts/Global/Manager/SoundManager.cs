using UnityEngine;

/// <summary>
/// 부팅 시 사운드 테이블을 로드하고 요청받은 소리를 재생하는 사운드 매니저입니다.
/// </summary>
public class SoundManager : GlobalSingleton<SoundManager>
{
    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private bool _isInitialized = false;
    private SoundTableSO _tableSfx;
    private SoundTableSO _tableBgm;
    private AudioSource _mainSource;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public override void Initialize()
    {
        if (_isInitialized)
        {
            return;
        }
        LoadSoundTable();
        // ↑ 필요한 초기화 로직 / 부모 클래스에서 자동 실행
        _isInitialized = true;
    }

    /// <summary>
    /// 메인 카메라에서 사운드를 재생합니다.
    /// </summary>
    public void PlaySfx(string id)
    {
        if (TryGetMainSource(out AudioSource mainSource)
            && TryGetSfxSO(id, out SoundSO soundSO))
        {
            PlaySfx(mainSource, soundSO);
        }
    }

    /// <summary>
    /// 좌표에서 사운드를 재생합니다.
    /// </summary>
    public void PlaySfx(string id, Vector3 pos)
    {
        if (TryGetMainSource(out AudioSource mainSource)
            && TryGetSfxSO(id, out SoundSO soundSO))
        {
            PlaySfx(mainSource, soundSO);
        }
    }

    /// <summary>
    /// 오브젝트에서 사운드를 재생합니다.
    /// </summary>
    public void PlaySfx(string id, Transform tr)
    {
        if (TryGetMainSource(out AudioSource mainSource)
            && TryGetSfxSO(id, out SoundSO soundSO))
        {
            PlaySfx(mainSource, soundSO);
        }
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    // 메인 오디오 소스를 갱신합니다.
    private bool TryGetMainSource(out AudioSource source)
    {
        if (_mainSource != null)
        {
            source = null;
            return true;
        }
        if (UCamera.MainCamera.TryGetComponent(out source))
        {
            _mainSource = source;
            UDebug.Print($"새로운 메인 오디오 소스를 캐싱했습니다.");
            return true;
        }
        UDebug.Print($"메인 오디오 소스를 찾지 못했습니다.", LogType.Assert);
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

    // BGM SO를 안전하게 가져오기
    private bool TryGetBgmSO(string id, out SoundSO so)
    {
        so = _tableBgm.GetSO(id);
        return so != null;
    }

    // SFX SO를 안전하게 가져오기
    private bool TryGetSfxSO(string id, out SoundSO so)
    {
        so = _tableSfx.GetSO(id);
        return so != null;
    }

    // 리소스 폴더에서 사운드 테이블을 로드
    private void LoadSoundTable()
    {
        // 사운드 테이블 우선 다 긁어오기
        var tables = Resources.LoadAll<SoundTableSO>(K.SOUND_RESOURCE_PATH);
        int length = tables.Length;
        bool success = true;
        // 순환하며 중복, 일치하지 않는 이름 등 검사하고 내부 변수에 주입하기
        for (int i = 0; i < length; ++i)
        {
            string name = tables[i].name;
            if (name == K.NAME_TABLE_SOUND_BGM)
            {
                if (_tableSfx == null)
                {
                    _tableSfx = tables[i];
                }
                else
                {
                    UDebug.Print($"해당 사운드 테이블이 이미 초기화되어 있습니다. ({name})", LogType.Assert);
                }
                continue;
            }
            if (name == K.NAME_TABLE_SOUND_SFX)
            {
                if (_tableBgm == null)
                {
                    _tableBgm = tables[i];
                }
                else
                {
                    UDebug.Print($"해당 사운드 테이블이 이미 초기화되어 있습니다. ({name})", LogType.Assert);
                }
                continue;
            }
            success = false;
            UDebug.Print($"사운드 테이블 로드 도중 일치하지 않는 이름을 발견했습니다. ({name})");
        }
        // 로그 출력
        if (success)
        {
            UDebug.Print($"사운드 테이블 로드 완료했습니다.");
        }
        else
        {
            UDebug.Print($"사운드 테이블 로드 실패했습니다.", LogType.Assert);
        }
    }
    #endregion
}
