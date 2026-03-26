using UnityEngine;

/// <summary>
/// 사용 시 고유 효과가 나타나는 아이템이 가지는 정적 데이터입니다.
/// </summary>
[CreateAssetMenu(fileName = "SpecialItemSO_", menuName = "ScriptableObjects/Item/Special", order = 1)]
public class SpecialItemSO : ItemSO
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("특수 아이템 정보")]
    [SerializeField] protected int _maxUseCount = 1; // 최대 사용 횟수
    [SerializeField] protected EffectSO[] _effects; // 아이템 효과
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public int MaxUseCount => _maxUseCount;

    /// <summary>
    /// 아이템 효과를 실행합니다.
    /// </summary>
    /// <param name="user">사용자 트랜스폼</param>
    /// <param name="target">목표 트랜스폼 (비우기 가능)</param>
    public void Use(Transform user, Transform target = null)
    {
        int length = _effects.Length;
        for (int i = 0; i < length; ++i)
        {
            _effects[i].Execute(user, target);
        }
    }

    // 정상 값을 가지는지 검사
    public override bool IsValid()
    {
        if (!base.IsValid()) return false;
        if (_type != EType.SpecialItem) return false;
        if (_maxUseCount <= 0) return false;
        if (_effects.Length > 0)
        {
            int length = _effects.Length;
            for (int i = 0; i < length; ++i)
            {
                if (_effects[i] == null)
                {
                    return false;
                }
            }
        }
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
