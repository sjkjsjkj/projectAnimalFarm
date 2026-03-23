using System;
using UnityEngine;

/// <summary>
/// 풀을 사용하는 객체가 상속받아야 하는 base 스크립트 입니다.
/// </summary>
public abstract class PoolItem : BaseMono
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("주제")]
    [SerializeField] private GameObject _prefab;
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
