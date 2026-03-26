using UnityEngine;

/// <summary>
/// 클래스의 설계 의도입니다.
/// </summary>
public abstract class InfoObject : Frameable
{
    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    public abstract void SetInfo(DatabaseUnitSO data);
    #endregion
}
