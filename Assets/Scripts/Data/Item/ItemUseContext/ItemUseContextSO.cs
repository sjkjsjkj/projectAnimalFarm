using UnityEngine;

/// <summary>
/// SO 클래스의 설계 의도입니다.
/// </summary>

public abstract class ItemUseContextSO : ScriptableObject
{

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public abstract bool TryUse();
    #endregion

}
