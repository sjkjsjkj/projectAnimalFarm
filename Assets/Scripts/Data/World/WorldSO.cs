using UnityEngine;

/// <summary>
/// SO 클래스의 설계 의도입니다.
/// </summary>
public class WorldSO : DatabaseUnitSO
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("기본 정보")]
    [SerializeField] private int _maxStack = 64; // 최대 중첩 수
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public int MaxStack => _maxStack;

    // 정상 값을 가지는지 검사
    public virtual bool IsValid()
    {
        if (!base.IsVaild()) return false;
        if (_maxStack <= 0) return false;
        return true;
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    // 인스펙터 변수 유효성 검사
    protected override void OnValidate()
    {
        if (!IsValid())
        {
            UDebug.PrintOnce($"SO({_id})의 값이 올바르지 않습니다.", LogType.Assert);
        }
    }
    #endregion
}
