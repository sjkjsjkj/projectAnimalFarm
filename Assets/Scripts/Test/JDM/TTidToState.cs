using UnityEngine;

/// <summary>
/// 클래스의 설계 의도입니다.
/// </summary>
public class TTidToState : BaseMono
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
    private void MoveHandle(OnPlayerMove ctx)
    {
        for (int i = -5; i < 10; ++i)
        {
            UDebug.Print($"타일 {i}번 : {TileIdToState.Ins[i]}");
        }
    }
    private void JumpHandle(OnPlayerJump ctx)
    {
        for (int i = 10; i < 20; ++i)
        {
            UDebug.Print($"타일 {i}번 : {TileIdToState.Ins[i]}");
        }
    }
    private void InteractHandle(OnPlayerInteract ctx)
    {
        for (int i = 9999999; i < 10000004; ++i)
        {
            UDebug.Print($"타일 {i}번 : {TileIdToState.Ins[i]}");
        }
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    private void OnEnable()
    {
        EventBus<OnPlayerJump>.Subscribe(JumpHandle);
        EventBus<OnPlayerMove>.Subscribe(MoveHandle);
        EventBus<OnPlayerInteract>.Subscribe(InteractHandle);
    }
    private void OnDisable()
    {
        EventBus<OnPlayerMove>.Unsubscribe(MoveHandle);
        EventBus<OnPlayerJump>.Unsubscribe(JumpHandle);
        EventBus<OnPlayerInteract>.Unsubscribe(InteractHandle);
    }
    #endregion

    #region ─────────────────────────▶ 중첩 타입 ◀─────────────────────────

    #endregion
}
