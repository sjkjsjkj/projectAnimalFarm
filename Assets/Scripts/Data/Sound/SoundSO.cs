using UnityEngine;

/// <summary>
/// 사운드를 올려두는 SO입니다.
/// </summary>
[CreateAssetMenu(fileName = "SoundSO_", menuName = "ScriptableObjects/Sound", order = 4)]
public class SoundSO : ScriptableObject
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("기본 정보")]
    [SerializeField] protected string _id = "";
    [SerializeField] protected AudioClip _clip;
    [SerializeField, Range(0f, 1f)] protected float _volume = 0.5f;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public string Id => _id;
    public AudioClip Clip => _clip;
    public float Volume => _volume;

    // 값 유효성 검사
    public virtual bool IsValid()
    {
        if (_id.IsEmpty()) return false;
        if (_clip == null) return false;
        if (_volume <= 0f) return false;
        return true;
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    private void AutomaticallyId()
    {
        // 클립 없거나 이름 이미 설정했으면 건드리지 않기
        if (_clip == null || _id.HasValue())
        {
            //return;
        }
        // 클립 이름 가져와서 Id에 넣기
        _id = _clip.name;
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    protected virtual void OnValidate()
    {
        AutomaticallyId();
        if (!IsValid())
        {
            UDebug.PrintOnce($"SO({_id})의 값이 올바르지 않습니다.", LogType.Assert);
        }
    }
    #endregion
}
