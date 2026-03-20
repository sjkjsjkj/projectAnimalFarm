using System;

/// <summary>
/// 클래스의 설계 의도입니다.
/// </summary>
public abstract class PoolItem : BaseMono
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    //[Header("주제")]
    //[SerializeField] private Class _class;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────

    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────

    public event Action<PoolItem> OnDead;
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    private void ResetState()
    {
        //상태들 리셋.
    }
    public abstract void SetInfo();
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    
    public void Dead()
    {
        ResetState();
        OnDead?.Invoke(this);
    }
    #endregion

    #region ─────────────────────────▶ 중첩 타입 ◀─────────────────────────

    #endregion
}
