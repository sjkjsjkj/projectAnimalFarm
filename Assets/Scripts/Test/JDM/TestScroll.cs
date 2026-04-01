/// <summary>
/// 클래스의 설계 의도입니다.
/// </summary>
public class TestScroll : BaseMono
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
        
    }
    private void JumpHandle(OnPlayerJump ctx)
    {
        UDebug.Print("점프 키 발생");
    }
    private void InteractHandle(OnPlayerInteract ctx)
    {
        UDebug.Print("상호작용 키 발생");
    }
    private void SlotHandle(OnPlayerSelectSlot ctx)
    {
        UDebug.Print("슬롯 키 발생 " + ctx.slot);
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    private void OnEnable()
    {
        EventBus<OnPlayerJump>.Subscribe(JumpHandle);
        EventBus<OnPlayerMove>.Subscribe(MoveHandle);
        EventBus<OnPlayerInteract>.Subscribe(InteractHandle);
        EventBus<OnPlayerSelectSlot>.Subscribe(SlotHandle);
    }
    private void OnDisable()
    {
        EventBus<OnPlayerMove>.Unsubscribe(MoveHandle);
        EventBus<OnPlayerJump>.Unsubscribe(JumpHandle);
        EventBus<OnPlayerInteract>.Unsubscribe(InteractHandle);
        EventBus<OnPlayerSelectSlot>.Unsubscribe(SlotHandle);
    }
    #endregion

    #region ─────────────────────────▶ 중첩 타입 ◀─────────────────────────

    #endregion
}
