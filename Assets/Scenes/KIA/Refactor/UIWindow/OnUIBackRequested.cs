/// <summary>
/// UI 뒤로가기 요청이 발생했을 때 발행되는 이벤트입니다.
/// Esc 입력, 모바일 뒤로가기 버튼 등을 하나로 묶기 위해 사용합니다.
/// </summary>
public readonly struct OnUIBackRequested
{
    #region ─────────────────────────▶ 외부 메서드 ◀─────────────────────────
    /// <summary>
    /// UI 뒤로가기 요청을 발행합니다.
    /// </summary>
    public static void Publish()
    {
        EventBus<OnUIBackRequested>.Publish(new OnUIBackRequested());
    }
    #endregion
}
