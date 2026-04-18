/// <summary>
/// 공용 피드백 메시지 표시 요청이 발생했을 때 발행되는 이벤트입니다.
/// </summary>
public readonly struct OnFeedbackMessageRequested
{
    #region ─────────────────────────▶ 상수 ◀─────────────────────────
    private const int MAX_MESSAGE_LENGTH = 40;
    #endregion

    #region ─────────────────────────▶ 읽기 전용 멤버 ◀─────────────────────────
    public readonly string message;
    public readonly EFeedbackMessageType messageType;
    public readonly float duration;
    #endregion

    #region ─────────────────────────▶ 생성자 ◀─────────────────────────
    /// <summary>
    /// OnFeedbackMessageRequested의 새 인스턴스를 초기화합니다.
    /// </summary>
    public OnFeedbackMessageRequested(string message, EFeedbackMessageType messageType, float duration)
    {
        this.message = message;
        this.messageType = messageType;
        this.duration = duration;
    }
    #endregion

    #region ─────────────────────────▶ 외부 메서드 ◀─────────────────────────
    /// <summary>
    /// 피드백 메시지 표시를 요청합니다.
    /// </summary>
    /// <param name="message">출력할 메시지</param>
    /// <param name="messageType">메시지 종류</param>
    /// <param name="duration">표시 유지 시간</param>
    public static void Publish(string message, EFeedbackMessageType messageType, float duration = 1.5f)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        if (messageType == EFeedbackMessageType.None)
        {
            return;
        }

        string safeMessage = message.Trim();

        if (safeMessage.Length <= 0)
        {
            return;
        }

        if (safeMessage.Length > MAX_MESSAGE_LENGTH)
        {
            return;
        }

        float safeDuration = UnityEngine.Mathf.Max(0f, duration);

        EventBus<OnFeedbackMessageRequested>.Publish(new OnFeedbackMessageRequested(safeMessage, messageType, safeDuration));
    }
    #endregion
}
