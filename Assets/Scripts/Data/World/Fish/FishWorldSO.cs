using UnityEngine;

/// <summary>
/// 물고기가 월드에서 가지는 정적 데이터입니다.
/// </summary>
[CreateAssetMenu(fileName = "FishWorldSO_", menuName = "ScriptableObjects/World/Fish", order = 1)]
public class FishWorldSO : WorldSO
{
    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    // 정상 값을 가지는지 검사
    public override bool IsValid()
    {
        if (!base.IsValid()) return false;
        if (_type != EType.FishWorld) return false;
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
            UDebug.PrintOnce($"SO({_id})의 값이 올바르지 않습니다.", LogType.Assert);
        }
    }
    #endregion
}
