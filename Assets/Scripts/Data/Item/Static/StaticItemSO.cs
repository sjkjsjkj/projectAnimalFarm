using UnityEngine;

/// <summary>
/// 추가적인 기능을 가지지 않는 아이템이 가지는 정적 데이터입니다.
/// </summary>
[CreateAssetMenu(fileName = "StaticItemSO_", menuName = "ScriptableObjects/Item/Static", order = 1)]
public class StaticItemSO : ItemSO
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
