using UnityEngine;

/// <summary>
/// 클래스의 설계 의도입니다.
/// </summary>
public class NewClass
{
    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private int _hp;
    private int _maxHp=100;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public int Hp {  get { return _hp; }
                    set {  _hp = Mathf.Clamp(value, Mathf.Max(0,_maxHp) , Mathf.Min(0, _hp)); }
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────

    #endregion
}
