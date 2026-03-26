using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 다수의 사운드 SO를 관리하는 테이블 클래스
/// </summary>
[CreateAssetMenu(fileName = "SoundTableSO_", menuName = "ScriptableObjects/Table/Sound", order = 1)]
public class SoundTableSO : ScriptableObject
{
    [Header("기본 정보")]
    [SerializeField] protected List<SoundSO> _sounds;

    private Dictionary<string, SoundSO> _soundDict;

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    /// <summary>
    /// 사운드 SO를 문자열 ID로 가져옵니다.
    /// </summary>
    /// <param name="id">오디오 ID</param>
    public SoundSO GetSO(string id)
    {
        if (_soundDict.TryGetValue(id, out SoundSO so))
        {
            return so;
        }
        UDebug.Print($"존재하지 않는 ID로 사운드 SO 인스턴스를 가져오려 시도했습니다.", LogType.Assert);
        return null;
    }

    /// <summary>
    /// 클립을 문자열 ID로 가져옵니다.
    /// </summary>
    /// <param name="id">오디오 ID</param>
    public AudioClip GetClip(string id)
    {
        if (_soundDict.TryGetValue(id, out SoundSO so))
        {
            return so.Clip;
        }
        UDebug.Print($"존재하지 않는 ID로 클립을 가져오려 시도했습니다.", LogType.Assert);
        return null;
    }

    /// <summary>
    /// 해당 클립의 개발자가 설정한 볼륨을 가져옵니다.
    /// </summary>
    /// <param name="id">오디오 ID</param>
    public float GetVolume(string id)
    {
        if (_soundDict.TryGetValue(id, out SoundSO so))
        {
            return so.Volume;
        }
        UDebug.Print($"존재하지 않는 ID로 볼륨을 가져오려 시도했습니다.", LogType.Assert);
        return 0f;
    }

    /// <summary>
    /// 해당 클립의 최종 볼륨을 계산해서 가져옵니다.
    /// </summary>
    /// <param name="id">오디오 ID</param>
    /// <param name="volume">사용자 볼륨 설정</param>
    public float CalcVolume(string id, float volume)
    {
        if (_soundDict.TryGetValue(id, out SoundSO so))
        {
            volume = Mathf.Max(volume, 0f);
            return Mathf.Min(so.Volume * volume, 1f);
        }
        UDebug.Print($"존재하지 않는 ID로 볼륨을 가져오려 시도했습니다.", LogType.Assert);
        return 0f;
    }

    // 값 유효성 검사
    public virtual bool IsValid()
    {
        if (UArray.IsInitedList(_sounds))
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// 딕셔너리 초기화를 위해 외부에서 호출해주는 메서드
    /// </summary>
    public void Initialize()
    {
        BuildSoundDict();
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    // 인스펙터의 리스트로 SoundSO를 담는 딕셔너리 작성
    private void BuildSoundDict()
    {
        if (UDebug.IsNull(_sounds))
        {
            return;
        }
        _soundDict = new();
        // 리스트 순회
        int length = _sounds.Count;
        int success = 0;
        for (int i = 0; i < length; ++i)
        {
            SoundSO so = _sounds[i];
            if (UDebug.IsNull(so))
            {
                continue;
            }
            // 딕셔너리 등록 시도
            if (!_soundDict.TryAdd(so.Id, so))
            {
                UDebug.Print($"사운드 SO 중복 등록이 감지되었습니다. (ID = {so.Id})");
                continue; // 실패 → 중복 등록
            }
            success++;
        }
        UDebug.Print($"사운드 테이블의 딕셔너리 작성을 완료했습니다. (총 {success}개)");
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    protected virtual void OnValidate()
    {
        if (!IsValid())
        {
            UDebug.PrintOnce($"사운드 테이블에 할당한 값이 올바르지 않습니다.", LogType.Assert);
        }
    }
    #endregion
}
