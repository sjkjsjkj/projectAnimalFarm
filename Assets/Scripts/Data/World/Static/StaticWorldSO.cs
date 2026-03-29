using UnityEngine;

/// <summary>
/// 특별한 기능이 없는 월드 오브젝트가 가지는 정적 데이터입니다.
/// </summary>
[CreateAssetMenu(fileName = "StaticWorldSO_", menuName = "ScriptableObjects/World/Static", order = 1)]
public class StaticWorldSO : WorldSO
{
    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    // 정상 값을 가지는지 검사
    public override bool IsValid()
    {
        if (!base.IsValid()) return false;
        return true;
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    // 인스펙터 변수 유효성 검사
    protected override void OnValidate()
    {
        base.OnValidate();
        if (!IsValid())
        {
            UDebug.PrintOnce($"SO({_id})의 값이 올바르지 않습니다.", LogType.Warning);
        }
    }
    #endregion
}
