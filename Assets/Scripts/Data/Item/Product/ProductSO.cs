using UnityEngine;

/// <summary>
/// SO 클래스의 설계 의도입니다.
/// </summary>
[CreateAssetMenu(fileName = "ProductSO_", menuName = "ScriptableObjects/Item/Product", order = 1)]
public class ProductSO : ItemSO
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
  
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    protected virtual void OnValidate()
    {
        if (!IsValid())
        {
            UDebug.PrintOnce($"SO 인스턴스({this.name})의 값이 올바르지 않습니다. (ID = {_id}, Type = {this.GetType().Name})", LogType.Warning);
        }
    }
    #endregion
}
