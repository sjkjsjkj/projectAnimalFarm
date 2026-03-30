using UnityEngine;

/// <summary>
/// 월드에서 상호작용 시 획득 가능한 오브젝트가 가지는 정적 데이터입니다.
/// </summary>
[CreateAssetMenu(fileName = "FieldWorldSO_", menuName = "ScriptableObjects/World/Field", order = 1)]
public class FieldWorldSO : WorldSO
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("필드 오브젝트 정보")]
    [SerializeField] protected string _acquiredId; // 획득하는 아이템
    [SerializeField] protected int _getCountMin; // 한 번에 획득할 수 있는 최소 개수
    [SerializeField] protected int _getCountMax; // 한 번에 획득할 수 있는 최대 개수
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public string GetAcquiredId => _acquiredId;
    public int GetCountMin => _getCountMin;
    public int GetCountMax => _getCountMax;

    // 정상 값을 가지는지 검사
    public override bool IsValid()
    {
        if (!base.IsValid()) return false;
        if (_acquiredId.IsEmpty()) return false;
        if (_getCountMin <= 0) return false;
        if (_getCountMax <= 0) return false;
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
