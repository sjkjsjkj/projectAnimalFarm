using UnityEngine;

/// <summary>
/// NPC가 월드에서 가지는 정적 데이터입니다.
/// </summary>
[CreateAssetMenu(fileName = "NpcWorldSO_", menuName = "ScriptableObjects/World/Npc", order = 1)]
public class NpcWorldSO : WorldSO
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("애니메이션")]
    [SerializeField, CsvIgnore] protected RuntimeAnimatorController _animController;

    [Header("대사")]
    [SerializeField, CsvIgnore] protected string[] _dialouge; 
    // NPC가 존재할 씬 ID?
    // NPC가 이동하는 포인트 좌표?
    // NPC가 이동하는 속도
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public RuntimeAnimatorController AnimController => _animController;

    // 정상 값을 가지는지 검사
    public override bool IsValid()
    {
        if (!base.IsValid()) return false;
        if (_type != EType.NpcWorld) return false;
        if (_animController == null) return false;
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
