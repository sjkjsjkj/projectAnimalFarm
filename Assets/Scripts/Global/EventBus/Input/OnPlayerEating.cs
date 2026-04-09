/// <summary>
/// 플레이어가 음식을 섭취하기 시작했을 때
/// </summary>
public readonly struct OnPlayerEating
{
    public readonly float eatingTime;

    public OnPlayerEating(float eatingTime)
    {
        this.eatingTime = eatingTime;
    }

    /// <param name="eatingTime">나무 좌표</param>
    public static void Publish(float eatingTime)
    {
        EventBus<OnPlayerEating>.Publish(new OnPlayerEating(eatingTime));
    }
}
