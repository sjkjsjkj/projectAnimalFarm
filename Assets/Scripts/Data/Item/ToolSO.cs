using UnityEngine;

/// <summary>
/// SO 클래스의 설계 의도입니다.
/// </summary>
[CreateAssetMenu(fileName = "ToolSO_", menuName = "ScriptableObjects/ToolSO", order = 1)]
public class ToolSO : DatabaseUnitSO
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("기본 정보")]
    [SerializeField] private EToolType _type;
    [SerializeField] private int _maxDurability = 100; // 최대 내구도
    [SerializeField] private float _strength = 1f; // 강도 (작업 효율, 캐는 속도 등)
    [SerializeField] private int _price = 100; // 가격
    [SerializeField] private int _maxStack = 1; // 동일 슬롯 최대 스택 수
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public EToolType ToolType => _type;
    public float MaxDurability => _maxDurability;
    public float Strength => _strength;
    public int Price => _price;
    public int MaxStack => _maxStack;

    // 값 유효성 검사
    public virtual bool IsValid()
    {
        if (!base.IsVaild()) return false;
        return true;
    }
    #endregion

    protected override void OnValidate()
    {
        base.OnValidate();
        if (!IsValid())
        {
            UDebug.PrintOnce($"SO({_id})의 값이 올바르지 않습니다.", LogType.Assert);
        }
    }
}
