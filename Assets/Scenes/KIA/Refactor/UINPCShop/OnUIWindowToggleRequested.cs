/// <summary>
/// 특정 UI 창 토글을 요청하는 이벤트입니다.
/// </summary>
public readonly struct OnUIWindowToggleRequested
{
    #region ─────────────────────────▶ 읽기 전용 멤버 ◀─────────────────────────
    public readonly EUIWindowId windowId;
    #endregion

    #region ─────────────────────────▶ 생성자 ◀─────────────────────────
    public OnUIWindowToggleRequested(EUIWindowId windowId)
    {
        this.windowId = windowId;
    }
    #endregion

    #region ─────────────────────────▶ 외부 메서드 ◀─────────────────────────
    /// <summary>
    /// UI 창 토글 요청을 발행합니다.
    /// </summary>
    /// <param name="windowId">토글할 창 식별자</param>
    public static void Publish(EUIWindowId windowId)
    {
        EventBus<OnUIWindowToggleRequested>.Publish(new OnUIWindowToggleRequested(windowId));
    }
    #endregion
}
