/// <summary>
/// 플레이어가 음식을 섭취하기 시작했을 때
/// </summary>
public readonly struct OnPlayerEating
{
    public readonly float eatingTime;
    public readonly float duration;

    public OnPlayerEating(float eatingTime, float duration)
    {
        this.eatingTime = eatingTime;
        this.duration = duration;
    }

    /// <param name="eatingTime">나무 좌표</param>
    /// /// <param name="duration">애니메이션 지속시간</param>
    public static void Publish(float eatingTime, float duration)
    {
        EventBus<OnPlayerEating>.Publish(new OnPlayerEating(eatingTime, duration));
    }
}
