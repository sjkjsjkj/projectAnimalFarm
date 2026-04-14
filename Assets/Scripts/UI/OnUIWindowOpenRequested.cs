/// <summary>
/// 특정 UI 창 열기를 요청하는 이벤트입니다.
/// </summary>
public readonly struct OnUIWindowOpenRequested
{
    #region ─────────────────────────▶ 읽기 전용 멤버 ◀─────────────────────────
    public readonly EUIWindowId windowId;
    #endregion

    #region ─────────────────────────▶ 생성자 ◀─────────────────────────
    public OnUIWindowOpenRequested(EUIWindowId windowId)
    {
        this.windowId = windowId;
    }
    #endregion

    #region ─────────────────────────▶ 외부 메서드 ◀─────────────────────────
    /// <summary>
    /// UI 창 열기 요청을 발행합니다.
    /// </summary>
    /// <param name="windowId">열 창 식별자</param>
    public static void Publish(EUIWindowId windowId)
    {
        EventBus<OnUIWindowOpenRequested>.Publish(new OnUIWindowOpenRequested(windowId));
    }
    #endregion
}
