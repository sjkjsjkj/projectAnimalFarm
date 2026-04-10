/// <summary>
/// 플레이어가 음료를 마시기 시작했을 때
/// </summary>
public readonly struct OnPlayerDrinking
{
    public readonly float drinkingTime;
    public readonly float duration;

    public OnPlayerDrinking(float drinkingTime, float duration)
    {
        this.drinkingTime = drinkingTime;
        this.duration = duration;
    }

    /// <param name="drinkingTime">나무 좌표</param>
    /// <param name="duration">애니메이션 지속시간</param>
    public static void Publish(float drinkingTime, float duration)
    {
        EventBus<OnPlayerDrinking>.Publish(new OnPlayerDrinking(drinkingTime, duration));
    }
}
