using UnityEngine;

/// <summary>
/// 도구 아이템이 가지는 정적 데이터입니다.
/// </summary>
[CreateAssetMenu(fileName = "ToolItemSO_", menuName = "ScriptableObjects/Item/Tool", order = 1)]
public class ToolItemSO : ItemSO
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("도구 아이템 정보")]
    [SerializeField] protected int _maxDurability = 100; // 최대 내구도
    [SerializeField] protected int _currentLv = 1; // 해당 도구의 레벨
    [SerializeField] protected float _range = 1f; // 도구를 사용할 수 있는 범위
    [SerializeField] protected float _strength = 1f; // 강도 (작업 효율, 캐는 속도 등)
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public int MaxDurability => _maxDurability;
    public int CurrentLv => _currentLv;
    public float Range => _range;
    public float Strength => _strength;

    // 정상 값을 가지는지 검사
    public override bool IsValid()
    {
        if (!base.IsValid()) return false;
        if (_maxDurability <= 0) return false;
        if (_currentLv <= 0) return false;
        if (_range <= 0f) return false;
        if (_strength < 0f) return false;
        if (_maxStack != 0) return false;
        return true;
    }
    #endregion

    // 인스펙터 변수 유효성 검사
    protected override void OnValidate()
    {
        base.OnValidate();
        if (!IsValid())
        {
            UDebug.PrintOnce($"SO 인스턴스({this.name})의 값이 올바르지 않습니다. (ID = {_id}, Type = {this.GetType().Name})", LogType.Warning);
        }
    }
}
