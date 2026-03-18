using UnityEngine;

/// <summary>
/// 클래스의 설계 의도입니다.
/// </summary>
public class TestKey : BaseMono
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    //[Header("주제")]
    //[SerializeField] private Class _class;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────

    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────

    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    private void JumpHandle(OnPlayerJump ctx)
    {
        UDebug.Print($"{ctx} 테스트");
    }
    private void MoveHandle(OnPlayerMove ctx)
    {
        UDebug.Print($"{ctx.moved} 테스트");
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    private void OnEnable()
    {
        EventBus<OnPlayerJump>.Subscribe(JumpHandle);
        EventBus<OnPlayerMove>.Subscribe(MoveHandle);
    }
    private void OnDisable()
    {
        EventBus<OnPlayerJump>.Unsubscribe(JumpHandle);
        EventBus<OnPlayerMove>.Unsubscribe(MoveHandle);
    }
    #endregion

    #region ─────────────────────────▶ 중첩 타입 ◀─────────────────────────

    #endregion
}
