using UnityEngine;

/// <summary>
/// 레시피가 가지는 정적 데이터입니다.
/// </summary>
[CreateAssetMenu(fileName = "RecipeSO_", menuName = "ScriptableObjects/Recipe/RecipeSO", order = 1)]
public class RecipeSO : BaseSO
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("기본 정보")]
    //만드는 아이템 아이디
    [SerializeField] private string _makeTargetItemId;
    [SerializeField] private EType _targetItemType;
    //필요한 아이템 목록
    [SerializeField] private RequireItemSO[] _requireItems;
    
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public string Id => _id;
    public string TargetItemId => _makeTargetItemId;
    public EType TargetItemType => _targetItemType;
    public RequireItemSO[] RequiedItems => _requireItems;


    // 값 유효성 검사
    public virtual bool IsValid()
    {
        if (_id.IsEmpty()) return false;
        if (_makeTargetItemId.IsEmpty()) return false;
        if (RequiedItems == null || RequiedItems.Length == 0) return false;

        return true;
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    protected virtual void OnValidate()
    {
        if (!IsValid())
        {
            UDebug.PrintOnce($"SO({_id})의 값이 올바르지 않습니다.", LogType.Warning);
        }
    }
    #endregion
}
