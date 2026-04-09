using UnityEngine;

/// <summary>
/// SO 클래스의 설계 의도입니다.
/// </summary>
[CreateAssetMenu(fileName = "ShopSO_", menuName = "ScriptableObjects/World/ShopSO", order = 1)]
public class ShopSO : WorldSO
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("상점 아이템 목록")]
    [SerializeField] protected ItemSO[] _shopItems;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public ItemSO[] ShopItems => _shopItems;

    // 값 유효성 검사
    public override bool IsValid()
    {
        base.IsValid();
        if (_shopItems==null || _shopItems.Length == 0) return false;
        
        return true;
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    protected override void OnValidate()
    {
        if (!IsValid())
        {
            UDebug.PrintOnce($"SO 인스턴스({this.name})의 값이 올바르지 않습니다. (ID = {_id}, Type = {this.GetType().Name})", LogType.Warning);
        }
    }
    #endregion
}
