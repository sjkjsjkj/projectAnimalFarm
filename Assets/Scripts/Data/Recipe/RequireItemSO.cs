using UnityEngine;

/// <summary>
/// 레시피에 들어갈 재료들의 정적 데이터 입니다.
/// </summary>
[CreateAssetMenu(fileName = "RequireItemSO_", menuName = "ScriptableObjects/Recipe/RequireItemSO", order = 1)]
public class RequireItemSO : ScriptableObject
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("기본 정보")]
    [SerializeField] private ItemSO _requireItem;
    [SerializeField] private int _count;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public ItemSO RequireItem => _requireItem; 
    public int Count => _count;
    
    // 값 유효성 검사
    public virtual bool IsValid()
    {
        if (_requireItem == null) return false;
        if (_count == 0) return false;
        return true;
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    protected virtual void OnValidate()
    {
        if (!IsValid())
        {
            UDebug.PrintOnce($"SO({name})의 값이 올바르지 않습니다.", LogType.Warning);
        }
    }
    #endregion
}
