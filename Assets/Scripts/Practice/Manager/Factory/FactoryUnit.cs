using UnityEngine;

/// <summary>
/// 각각의 팩토리들을 구현할 클래스입니다.
/// </summary>
public abstract class FactoryUnit : BaseMono
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [SerializeField] protected GameObject _prefab;

    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────

    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────

    #endregion
    #region ─────────────────────────▶  생성자  ◀─────────────────────────

    #endregion
    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    public abstract GameObject Spawn(string id);
    
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────

    #endregion

    #region ─────────────────────────▶ 중첩 타입 ◀─────────────────────────

    #endregion
}
