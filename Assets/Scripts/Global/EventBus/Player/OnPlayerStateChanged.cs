/// <summary>
/// 플레이어의 상태가 변경되었을 때
/// </summary>
public readonly struct OnPlayerStateChanged
{
    public readonly EPlayerState prevState;
    public readonly EPlayerState nextState;

    public OnPlayerStateChanged(EPlayerState prevState, EPlayerState nextState)
    {
        this.prevState = prevState;
        this.nextState = nextState;
    }

    /// <param name="prevState">이전 상태</param>
    /// <param name="nextState">현재 상태</param>
    public static void Publish(EPlayerState prevState, EPlayerState nextState)
    {
        EventBus<OnPlayerStateChanged>.Publish(new(prevState, nextState));
    }
}
